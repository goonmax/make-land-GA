using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Media;

namespace MakeLand
{
    public class Population
    {
        public int generation = 0;
        public int bestScore = 0;
        public int bestIndex = 0;
        public int numInPop = 0;

        public int[] listOfLiving;
        public int countOfLiving;

        public Phenotype[] maps = null;

        public Population(int numInPopZ, Random r)
        {
            numInPop = numInPopZ;
            maps = new Phenotype[numInPop];
            for (int i = 0; i < numInPop; i++)
            {
                Genotype g = new Genotype(r);
                Phenotype p = new Phenotype(g,0);
                p.createPheno();
                p.setScore();
                maps[i] = p; 
            }
        }

        /// <summary>
        /// Returns the index of the best individual and updates bestScore
        /// </summary>
        /// <returns></returns>
        public int findBest()
        {
            Phenotype p = maps[0];
            bestScore = p.score;
            bestIndex = 0;
            for (int i = 1; i < numInPop; i++)
            {
                p = maps[i];
                if (p.score > bestScore)
                {
                    bestIndex = i;
                    bestScore = p.score;
                }
            }
            return bestIndex;
        }


        /// <summary>
        /// Finds the worst individual thats actually alive
        /// </summary>
        /// <returns></returns>
        public int findWorstAlive()
        {
            bool first = true;
            int worstScore = 0;
            int worstIndex = 0;

            for (int i = 1; i < numInPop; i++)
            {
                Phenotype p = maps[i];
                if (p.alive && first)
                {
                    first = false;
                    worstScore = p.score;
                    worstIndex = i;
                    continue;
                }

                if (p.alive && p.score < worstScore)
                {
                    worstScore = p.score;
                    worstIndex = i;
                }
            }
            return worstIndex;
        }

        /// <summary>
        /// Just a standard getter
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Phenotype getPhenotype(int i)
        {
            return maps[i];
        }

        /// <summary>
        /// Unsets the newborn flag for the entire population
        /// </summary>
        public void unsetNewborn()
        {
            for (int i = 1; i < numInPop; i++)
            {
                getPhenotype(i).newborn = false;
            }
        }

        /// <summary>
        /// Kills the weakest
        /// </summary>
        /// <param name="n"></param>
        public void killThisMany(int n)
        {

            for (int i = 0; i <n; i++)
            {
                int k = findWorstAlive();
                getPhenotype(k).alive = false;
            }
        }



        /// <summary>
        /// Search for dead individuals - replace them with living newborn ones
        /// </summary>
        public void breedPopulation(Random r)
        {
            listOfLiving = new int[Params.populationCnt];
            countOfLiving=0;
            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (getPhenotype(i).alive && (!getPhenotype(i).newborn))
                {
                    listOfLiving[i] = i;
                    countOfLiving++;
                }
            }

            for (int i = 0; i < Params.populationCnt; i++)
            {
                if (!getPhenotype(i).alive)
                {
                    int mum = r.Next(0, countOfLiving);
                    int dad = r.Next(0, countOfLiving);
                    mum = listOfLiving[mum];
                    dad = listOfLiving[dad];
                    Phenotype mumP = getPhenotype(mum);
                    Phenotype dadP = getPhenotype(dad);
                    Genotype ggg = makeGenome(mumP.genotype,dadP.genotype);
                    if (Params.mutationPercent > r.Next(0,100)) mutate(ggg, r);
                    //checkDuplicateGenes(ggg);
                    maps[i] = new Phenotype(ggg, G.pop.generation);

                }
            }
        }


        public bool checkDuplicateGenes(Genotype ggg)
        {
            bool retv = false;
            for (int i = 0; i < Params.genotypeSize; i++)
                for (int k = i+1; k < Params.genotypeSize; k++)
                {
                    if(ggg.genes[i].equal(ggg.genes[k]))
                      {
                        G.dupGeneCount++;
                        ggg.genes[i] = new Gene(G.rnd);
                        retv = true;
                      }
                }
            return retv;
        }

        public void mutate(Genotype g, Random r)
        {
            G.mutationCount++;
            Random rnd = new Random();
            int x = rnd.Next(0, Params.genotypeSize);
            // this rolls a new gene 
            g.genes[x] = new Gene(r);


            //this amkes a small change to x & y 
            x = rnd.Next(0, Params.genotypeSize);
            if (G.rnd.NextDouble() < 0.5)
            {
                if (g.genes[x].x < Params.dimX)
                    g.genes[x].x++;
                if (g.genes[x].y < Params.dimY)
                    g.genes[x].y++;
            }
            else
            {
                if (g.genes[x].x > 0)
                    g.genes[x].x--;
                if (g.genes[x].y > 0)
                    g.genes[x].y--;
            }
            //small cahnge to x and y repeat 
            x = rnd.Next(0, Params.genotypeSize);
            if (G.rnd.NextDouble() < 0.5)
            {
                if (g.genes[x].repeatX < Params.maxRepeat)
                    g.genes[x].repeatX++;
                if (g.genes[x].y < Params.maxRepeat)
                    g.genes[x].repeatX++;
            }
            else
            {
                if (g.genes[x].x > 0)
                    g.genes[x].repeatX--;
                if (g.genes[x].y > 0)
                    g.genes[x].repeatY--;
            }
            //swap two of the genes to make em random :L
            x = rnd.Next(0, Params.genotypeSize / 2);
            Gene temp = g.genes[x];
            int y = rnd.Next(Params.genotypeSize / 2, Params.genotypeSize);
            g.genes[x] = g.genes[y];
            g.genes[x].terrain = 1;
           // if (g.genes[x].repeatX < Params.maxRepeat)
             //   g.genes[x].re
        }

        /// <summary>
        /// create a new geneome from mum and dad
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public Genotype makeGenome(Genotype g1, Genotype g2)
        {
            Genotype retv = new Genotype();
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (G.rnd.NextDouble()<0.5)
                {
                    retv.genes[i] = new Gene(g1.genes[i]);
                }
                else
                {
                    retv.genes[i] = new Gene(g2.genes[i]);
                }
            }
            return retv;
        }

        public void checkDuplicateGenotypes()
        {
            for (int i = 0; i < Params.populationCnt; i++)
            {
                Genotype g = getPhenotype(i).genotype;
                if (checkDuplicateGenes(g)) continue;
                for (int k = i + 1; k < Params.populationCnt; k++)
                {
                    Genotype kk = getPhenotype(k).genotype;
                    if (kk.equal(g))
                    {
                        mutate(g, G.rnd);
                        G.dupGeneomeCount++;
                    }
                }
            }
        }


        /// <summary>
        /// what it sounds like
        /// </summary>
        public void do1Generation()
        {
            G.pop.generation++;
            unsetNewborn();
            killThisMany(Params.populationCnt / 2);
            breedPopulation(G.rnd);
            //if (Params.checkDuplicateGenomes != -1 && G.pop.generation % Params.checkDuplicateGenomes == 0) checkDuplicateGenotypes(); 
            Application.DoEvents();
        }

    }

    public class Genotype
    {
        public Gene[] genes = new Gene[Params.genotypeSize];

        public Genotype(Random r)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene(r);
        }

        public Genotype()
        {
            for (int i = 0; i < Params.genotypeSize; i++)
                genes[i] = new Gene();
        }

        public bool equal(Genotype gg)
        {
            for (int i = 0; i < Params.genotypeSize; i++)
            {
                if (!(gg.genes[i].equal(genes[i]))) return false;
            }
            return true;
        }
    }



    public class Gene
    {
        public int terrain=0;
        public int x=0;
        public int y=0;
        public int repeatY = 0;
        public int repeatX = 0;

        public Gene()
        {

        }

        public Gene(int ter, int xx, int yy, int rptX, int rptY)
        {
            terrain = ter;
            x = xx;
            y = yy;
            repeatX = rptX;
            repeatY = rptY;
        }

        /// <summary>
        /// New Random Gene
        /// </summary>
        /// <param name="r"></param>
        public Gene(Random r)
        {
            terrain = r.Next(0,3);
            x = r.Next(0, Params.dimX);
            y = r.Next(0, Params.dimY);
            repeatX = r.Next(0, Params.maxRepeat);
            repeatY = r.Next(0, Params.maxRepeat);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="gg"></param>
        public Gene(Gene gg) // copy constructor
        {
            terrain = gg.terrain;
            x = gg.x;
            y = gg.y;
            repeatX = gg.repeatX;
            repeatY = gg.repeatY;
        }

        public bool equal(Gene g)
        {
            if (g.x != x) return false;
            if (g.y != y) return false;
            if (g.repeatX != repeatX) return false;
            if (g.repeatY != repeatY) return false;
            if (g.terrain != terrain) return false;
            return true;
        }

    }

    public class Phenotype
    {
        public Genotype genotype=null; // reference class - this is a pointer not a copy
        int[,] pheno = null;
        Bitmap bitm = null;
        public int score = 0;
        public bool alive = true;
        public bool newborn = true;
        public int gen = 0; 

        /// <summary>
        /// Default constructor probably not helpfull
        /// </summary>
        public Phenotype()
        {
            // default is all null - no need for code yet
        }

        /// <summary>
        /// This is the critical constructor it creates the pheno array for scoring
        /// </summary>
        /// <param name="gg"></param>
        public Phenotype(Genotype gg, int generationCount)
        {
            genotype = gg;
            createPheno();
            setScore();
            gen = generationCount;

        }

        /// <summary>
        ///  create the pheno array
        /// </summary>
        public void createPheno()
        {
            pheno = new int[Params.dimX, Params.dimY];
            for (int x=0; x< Params.dimX;x++)
                for (int y=0; y< Params.dimY;y++) { pheno[x,y] = 0; } // initialise to 0

            for (int i = 0; i < Params.genotypeSize; i++)
            {
                Gene g = genotype.genes[i];
                for (int kx = 0; kx < g.repeatX; kx++)
                    for (int ky = 0; ky < g.repeatY; ky++)
                    {
                        int x = g.x+kx;
                        int y = g.y+ky;
                    if (y< Params.dimY && x< Params.dimX) pheno[x, y] = g.terrain;
                }

            }
        }

        public int getTerrainSafe(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Params.dimX || y >= Params.dimY) return 0;
            return pheno[x, y];
        }

        /// <summary>
        /// returns the score for selection - also stores it in Phenotype
        /// </summary>
        /// <returns></returns>
        public int setScore()
        {
            score = 0;

            int seaCount = 0;
            int landCount = 0;
            int freshCount = 0;

            int every = 1;
            int edgewidth = Params.dimX / 16;
            //left
            for (int y = 0; y < Params.dimY; y = y + every)
            {
                for (int x = 0; x < edgewidth; x = x + every)
                {
                    if (pheno[x, y] != 0)
                    {
                        score -= 30;
                        continue;
                    }
                    if (pheno[x, y].Equals(pheno[x + 1, y]))
                        score++;
                }
            }
            //top        
            for (int x = 0; x < Params.dimX; x = x + every)
            {
                for (int y = 0; y < edgewidth; y = y + every)
                {
                    if (pheno[x, y] != 0)
                    {
                        score -= 30;
                        continue;
                    }
                    if (pheno[x, y].Equals(pheno[x, y]))
                        score++;
                }

            }
            //right
            for (int y = 0; y < Params.dimY; y = y + every)
            {
                for (int x = Params.dimX - 1; x > Params.dimX - edgewidth; x = x - every)
                {
                    if (pheno[x, y] != 0)
                    {
                        score -= 30;
                        continue;
                    }
                    if (pheno[x, y].Equals(pheno[x - 1, y]))
                        score++;
                }
            }
            //bottem
            for (int x = 0; x < Params.dimX; x = x + every)
            { 
                for (int y = Params.dimY - 1; y > Params.dimY - edgewidth; y = y - every)
                {
                    if (pheno[x, y] != 0)
                    {
                        score -= 30;
                        continue;
                    }
                    if (pheno[x, y].Equals(pheno[x, y - 1]))
                        score++;
                }
            }


            // if pheno connects to more of itself 
            for (int x = edgewidth; x < Params.dimX - edgewidth; x = x + every)
            {
                for (int y = edgewidth; y < Params.dimY - edgewidth; y = y + every)
                {
                    //water 
                    if (pheno[x, y] == 0)
                    {
                        int temp = 0;
                        if (pheno[x, y].Equals(pheno[x - 1, y]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x, y - 1]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x + 1, y]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x, y + 1]))
                            temp++;
                        if (temp < 3)
                            score -= 15;
                    }
                    //land
                    else if (pheno[x, y] == 1)
                    {
                        int temp = 0;
                        if (pheno[x, y].Equals(pheno[x - 1, y]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x, y - 1]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x + 1, y]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x, y + 1]))
                            temp++;
                        if (temp >= 1)
                            score += 5; //had this at 5 before wasnt m,aking enough land boii 
                        else
                            score -= 5;//also had this at 5 before wasnt m,aking enough land boii 
                    }
                    //more water that be fresh, lad 
                    else if (pheno[x, y] == 2)
                    {
                        int temp = 0;
                        if (pheno[x, y].Equals(pheno[x - 1, y]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x, y - 1]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x + 1, y]))
                            temp++;
                        if (pheno[x, y].Equals(pheno[x, y + 1]))
                            temp++;
                        if (temp < 3)
                            score -= 2;
                    }
                }
            }
            // counting the terrain yeah yeet
            for (int x = edgewidth; x < Params.dimX - edgewidth; x = x + every)
            {
                for (int y = edgewidth; y < Params.dimY - edgewidth; y = y + every)
               {
                    int temp = pheno[x, y];
                    if (temp == 0)
                    {
                        seaCount++;
                        score--;// changed -- to ++
                    }
                    else if (temp == 1)
                    {
                        landCount++;
                        score++;
                    }
                    else if (temp == 2)
                        freshCount++;// changed ++ to --
                }
            }
            int reducedTheWidth = (Params.dimX - (edgewidth * 2)) * (Params.dimY - (edgewidth * 2));
            if ((landCount / reducedTheWidth) < Params.percentLand)
                score -= 200;
            else if ((landCount / reducedTheWidth) > Params.percentLand + 0.1)
                score -= 10;
            else
                score += 500;
            if ((freshCount / reducedTheWidth) > Params.percentFresh)
                score -= 200;
            else
                score += 500;
            //checking the line that go down to see how many times the terrain change 
            for (int x = edgewidth; x < Params.dimX - edgewidth; x = x + every)
            {
                int temp = -1;
                int count = -1;
                for (int y = edgewidth; y < Params.dimY - edgewidth; y = y + every)
                {
                    if (pheno[x, y] != temp)
                    {
                        temp = pheno[x, y];
                        count++;
                    }
                }
                if (count >= 5 && count <= 8)
                    score += 5;
                else
                    score -= 50;
            }

            //check the line that go across to see how many times the terrain cahnges horizontal
            for (int y = edgewidth; y < Params.dimX - edgewidth; y = y + every)
            {
                int temp = -1;
                int count = -1;
                for (int x = edgewidth; x < Params.dimY - edgewidth; x = x + every)
                {
                    if (pheno[x, y] != temp)
                    {
                        temp = pheno[x, y];
                        count++;
                    }
                }
                if (count >= 5 && count <= 8)
                    score += 5;
                else
                    score -= 50;
            }
            return score;
            //return 0;
        }

        /// <summary>
        /// Display the map in a picturebox
        /// </summary>
        public void show(PictureBox pb)
        {
            System.Drawing.SolidBrush myBrush;
            if (bitm == null)
            {
                bitm = new Bitmap(Params.dimX, Params.dimY);
                myBrush = new System.Drawing.SolidBrush(G.ca[0]);
                Graphics gra = Graphics.FromImage(bitm);

                gra.FillRectangle(myBrush,0,0, Params.dimX, Params.dimY); //this is your code for drawing rectangles
                
                for (int x=0; x< Params.dimX; x++)
                {
                    for (int y = 0; y < Params.dimY; y++)
                    {
                        if (pheno[x,y] > 0)
                        {
                            bitm.SetPixel(x, y, G.ca[pheno[x,y]]);
                        }
                    }
                }
            }
            pb.Image = bitm;
        }
    }
}
