/**********************************
 * Renaud Meurs
 * V0.2
 * 13/12/13 : Création
 * 26/12/13 : Ajout des méthodes map.getFullMapCells(), setMapCells(byteArray)
 * 
 */
using System;
using System.Runtime.InteropServices;

//  Sachant que je balaye la carte ligne par ligne et de gauche a droite : 
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stMapCell          //  Une cellule d'une carte 
{
    public byte sPosX;         //  Début position X.
    public byte sPosY;         //  Début position Y.
    public byte ePosY;         //  Fin position Y. Vaut 0 si c'est un batiment. Différent de 0 si c'est un mur. Différent de debut position Y si c'est un mur contigu du même niveau
    public byte ePosX;         //   Fin position X pour Fabrice :( 
    public byte idBuilding;    //  Byte identifiant le type d'élément présent sur la cellule. Donc 255 type d'éléments différents possible.
    public byte levelBuilding; //  Le niveau de l'élément présent sur la cellule.
}   //  6 Bytes
// Dans le cas d'un élément autre qu'un mur, je n'ai qu'une occurence de MapCell qui représente sont point de départ. Le reste est déterminé par un tableau
// décrivant le batiment ou la décoration. Je parle donc de largeur et de profondeur.

public struct stFullMapCell
{
    public byte sPosX;          //  Début position X.
    public byte sPosY;          //  Début position Y.
    //public byte ePosY;          //  Fin position Y. Vaut 0 si c'est un batiment. Différent de 0 si c'est un mur. Différent de debut position Y si c'est un mur contigu du même niveau
    public byte ePosX;          //  Fin position X.
    public byte idBuilding;     //  Byte identifiant le type d'élément présent sur la cellule. Donc 255 type d'éléments différents possible.
    public byte levelBuilding;  //  Le niveau de l'élément présent sur la cellule.
    public int life;            //  Point de vie du batiment. 
    public byte width;          //  Largeur du batiment.
    public byte lenght;         //  Longueur du batiment.
}
    
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stCounters     //  Les compteurs.
{
    public int gold;       //  Solde or.
    public int oil;        //  Solde pétrole.
    public int diamond;    //  Solde diamant.
    /* ........ */
}   //  12 Bytes

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stPendingEvolution //  Les évolutions de bâtiment en cours.
{
    public short idMapCell;    //  Coordonnées de l'élément en cours d'évolution.
    public long start;         //  Datetime de lancement de l'évolution.
    public int evolutionTime;  //  Temps de l'évolution en secondes.
}   //   Bytes

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stPendingResearch  //  Les recherches en cours, en imaginant que nous n'avonc qu'un laboratoire.
{
    public long start;         //  Datetime de lancement de la recherche.
    public int researchTime;   //  Temps de la recherche en seconde
}   //  22 Bytes

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stMineLoad         //  Les montants des ressources stockés dans les mines.
{
    public short idMapCell;    //  Coordonnées de la mine.
    public long tsLastDump;    //  Timestamp de la dernière récolte.
    public short value;        //  Montant de ressources contenues dans la mine.
}   //  12 Bytes

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stCampUnit         //  Les unités présentes dans les camps d'entrainement.
{
    public short idMapCell;    //  Coordonées du camp d'entrainement
    public byte idUnit;        //  Identifiant de l'unité. Donc max 255 type d'unités différentes. Les identifiants sont hard codé dans le programme.
    public byte unitLevel;     //  Level de l'unité concernée.
    public byte nbUnit;        //  Nombre d'unités dans le camp.
}   //  5 Bytes

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stTrainingQueue    //  Représente un type d'unité dans la fille d'attente de formation.
{
    public byte idUnit;        //  Type de Unit en formation.
    public short trainingTime; //  Durée de formation en secondes.
    public byte nbUnit;        //  Nombre de Unit restant dans la file d'attente.
}   //  4 Bytes

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct stPendingTraining  //  Les unités en cours de fomrmation.
{
    public short idMapCell;    //  Coordonnées de la caserne.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public stTrainingQueue[] queue;  //  Tableau des files d'attentes. 5 type différents d'unités en formation.

    public stPendingTraining(int i)
    {
        this.idMapCell = 0;
        this.queue = new stTrainingQueue[5];
    }
}   //  22  Bytes

[StructLayout(LayoutKind.Sequential, Pack=1)]
public struct stMap               //  Contient toutes les données d'une carte.
{
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 300)]
    public stMapCell[] mapCells;              //  Tous les éléments de la carte sont stocké ici.

    public stCounters counters;               //  Tous les compteurs

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public stPendingEvolution[] evolutions;   //  Les évolutions en cours.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public stPendingResearch[] researches;    //  Les recherches en cours.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public stMineLoad[] mineLoads;            //  Les valeurs en ressources contenues dans les mines.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public stCampUnit[] campUnits;            //  Les unités dans les camps.

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
    public stPendingTraining[] trainings;     //  Les unités en cours de formation.

    public stMap(int i) {
        this.mapCells = new stMapCell[300];
        this.counters.diamond = 0;
        this.counters.gold = 0;
        this.counters.oil = 0;
        this.evolutions = new stPendingEvolution[5];
        this.researches = new stPendingResearch[2];
        this.mineLoads = new stMineLoad[12];
        this.trainings = new stPendingTraining[7];
        this.campUnits = new stCampUnit[60];
    }

    public byte[] serializeMapCells()
    {
        byte[] rawData = new byte[1800];
        IntPtr[] tbPtr = new IntPtr[300];

        for (int i = 0; i < this.mapCells.Length; i++)
        {
            tbPtr[i] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(stMapCell)));
            Marshal.StructureToPtr(this.mapCells[i], tbPtr[i], true);
            Marshal.Copy(tbPtr[i], rawData, i*6, 6);
            Marshal.FreeHGlobal(tbPtr[i]);
        }
        return rawData;
    }

    public void setMapCells(byte[] rawData)
    {
        int len = Marshal.SizeOf(typeof(stMapCell));

        for (int i = 0; i < this.mapCells.Length; i++)
        {
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(rawData, i * 6, ptr, 6);
            this.mapCells[i] = (stMapCell)Marshal.PtrToStructure(ptr, typeof(stMapCell));
            Marshal.FreeHGlobal(ptr);
        }
    }

    public byte[] serializeEvolutions()
    {
        int stSize = Marshal.SizeOf(typeof(stPendingEvolution));
        byte[] rawData = new byte[stSize * this.evolutions.Length];

        IntPtr[] tbPtr = new IntPtr[this.evolutions.Length];

        for (int i = 0; i < this.evolutions.Length; i++)
        {
            tbPtr[i] = Marshal.AllocHGlobal(stSize);
            Marshal.StructureToPtr(this.evolutions[i], tbPtr[i], true);
            Marshal.Copy(tbPtr[i], rawData, i * stSize, stSize);
            Marshal.FreeHGlobal(tbPtr[i]);
        }
        return rawData;
    }

    public void setEvolutions(byte[] rawData)
    {
        int len = Marshal.SizeOf(typeof(stPendingEvolution));

        for (int i = 0; i < this.evolutions.Length; i++)
        {
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(rawData, i * len, ptr, len);
            this.evolutions[i] = (stPendingEvolution)Marshal.PtrToStructure(ptr, typeof(stPendingEvolution));
            Marshal.FreeHGlobal(ptr);
        }
    }

    public byte[] serializeResearches()
    {
        int stSize = Marshal.SizeOf(typeof(stPendingResearch));
        byte[] rawData = new byte[stSize * this.researches.Length];
        IntPtr[] tbPtr = new IntPtr[this.researches.Length];

        for (int i = 0; i < this.researches.Length; i++)
        {
            tbPtr[i] = Marshal.AllocHGlobal(stSize);
            Marshal.StructureToPtr(this.researches[i], tbPtr[i], true);
            Marshal.Copy(tbPtr[i], rawData, i * stSize, stSize);
            Marshal.FreeHGlobal(tbPtr[i]);
        }
        return rawData;
    }

    public void setResearches(byte[] rawData)
    {
        int len = Marshal.SizeOf(typeof(stPendingResearch));

        for (int i = 0; i < this.researches.Length; i++)
        {
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(rawData, i * len, ptr, len);
            this.researches[i] = (stPendingResearch)Marshal.PtrToStructure(ptr, typeof(stPendingResearch));
            Marshal.FreeHGlobal(ptr);
        }
    }

    public byte[] serializeMineLoads()
    {
        int stSize = Marshal.SizeOf(typeof(stMineLoad));
        byte[] rawData = new byte[stSize * this.mineLoads.Length];
        IntPtr[] tbPtr = new IntPtr[this.mineLoads.Length];

        for (int i = 0; i < this.mineLoads.Length; i++)
        {
            tbPtr[i] = Marshal.AllocHGlobal(stSize);
            Marshal.StructureToPtr(this.mineLoads[i], tbPtr[i], true);
            Marshal.Copy(tbPtr[i], rawData, i * stSize, stSize);
            Marshal.FreeHGlobal(tbPtr[i]);
        }
        return rawData;
    }

    public void setMineLoads(byte[] rawData)
    {
        int len = Marshal.SizeOf(typeof(stMineLoad));

        for (int i = 0; i < this.mineLoads.Length; i++)
        {
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(rawData, i * len, ptr, len);
            this.mineLoads[i] = (stMineLoad)Marshal.PtrToStructure(ptr, typeof(stMineLoad));
            Marshal.FreeHGlobal(ptr);
        }
    }

    public byte[] serializeCampUnits()
    {
        int stSize = Marshal.SizeOf(typeof(stCampUnit));
        byte[] rawData = new byte[stSize * this.campUnits.Length];
        IntPtr[] tbPtr = new IntPtr[this.campUnits.Length];

        for (int i = 0; i < this.campUnits.Length; i++)
        {
            tbPtr[i] = Marshal.AllocHGlobal(stSize);
            Marshal.StructureToPtr(this.campUnits[i], tbPtr[i], true);
            Marshal.Copy(tbPtr[i], rawData, i * stSize, stSize);
            Marshal.FreeHGlobal(tbPtr[i]);
        }
        return rawData;
    }

    public void setCampUnits(byte[] rawData)
    {
        int len = Marshal.SizeOf(typeof(stCampUnit));

        for (int i = 0; i < this.campUnits.Length; i++)
        {
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(rawData, i * len, ptr, len);
            this.campUnits[i] = (stCampUnit)Marshal.PtrToStructure(ptr, typeof(stCampUnit));
            Marshal.FreeHGlobal(ptr);
        }
    }

    public byte[] serializeTrainings()
    {
        int stSize = Marshal.SizeOf(typeof(stPendingTraining));
        byte[] rawData = new byte[stSize * this.trainings.Length];
        IntPtr[] tbPtr = new IntPtr[this.trainings.Length];

        for (int i = 0; i < this.trainings.Length; i++)
        {
            tbPtr[i] = Marshal.AllocHGlobal(stSize);
            Marshal.StructureToPtr(this.trainings[i], tbPtr[i], true);
            Marshal.Copy(tbPtr[i], rawData, i * stSize, stSize);
            Marshal.FreeHGlobal(tbPtr[i]);
        }
        return rawData;
    }

    public void setTrainings(byte[] rawData)
    {
        int len = Marshal.SizeOf(typeof(stPendingTraining));

        for (int i = 0; i < this.trainings.Length; i++)
        {
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(rawData, i * len, ptr, len);
            this.trainings[i] = (stPendingTraining)Marshal.PtrToStructure(ptr, typeof(stPendingTraining));
            Marshal.FreeHGlobal(ptr);
        }
    }

    public stFullMapCell[] getFullMapCells()
    {
        stFullMapCell[] fullMapCells = new stFullMapCell[this.mapCells.Length];

        for (int i = 0; i < fullMapCells.Length; i++)
        {
            fullMapCells[i].sPosX = this.mapCells[i].sPosX;
            fullMapCells[i].ePosX = this.mapCells[i].ePosX;
            fullMapCells[i].sPosY = this.mapCells[i].sPosY;
            //fullMapCells[i].ePosY = this.mapCells[i].ePosY;
            fullMapCells[i].lenght = 0; // to do
            fullMapCells[i].width = 0;// to do
            fullMapCells[i].idBuilding = this.mapCells[i].idBuilding;
            fullMapCells[i].levelBuilding = this.mapCells[i].levelBuilding;
            fullMapCells[i].life = 0;// to do
        }
        return fullMapCells;
    }
}

public struct stBuilding
{
    public byte width;
    public byte lenght;
    public short lvl1Life;
    public short lvl2Life;
    public short lvl3Life;
    public short lvl4Life;
    public short lvl5Life;
    public short lvl6Life;
    public short lvl7Life;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class cStoredFullGame
{
    public int idGame;

    public stMap Map1;    //  Contient toutes les données de la première carte.
    
    public stMap Map2;    //  Contient toutes les données de la seconde carte.
    
	public cStoredFullGame()
	{
        this.Map1 = new stMap(1);
        this.Map2 = new stMap(1);
    }


}
