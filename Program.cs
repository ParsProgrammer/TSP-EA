using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TSP_EA
{
    class CityPoint
    {
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
     class Distance_Cities
     {
        private List<CityPoint> Cordination = new List<CityPoint>();
        private static int[,] Distance_Matrix = new int[127, 127];        
        public  Distance_Cities()
        {
            string line;          
            StreamReader file = new StreamReader("TSPDATA.txt");
            while ((line = file.ReadLine()) != null)
            {
                string[] subs = line.Split(' ');
                CityPoint ct = new CityPoint();
                List<string> ne = new List<string>();
                foreach (var s in subs)
                {            
                    if (s != "")                 
                       ne.Add(s);                          
                }
                ct.ID = Convert.ToInt16(ne[0]);
                ct.X = Convert.ToInt16(ne[1]);
                ct.Y = Convert.ToInt16(ne[2]);
                Cordination.Add(ct);

            }

        }
        public void SetDistanceMatrix()
        {
            for (int i = 0; i < 127; i++)
            {
                for (int j = 0; j < 127 ; j++)
                {
                    if(i!=j)
                     Distance_Matrix[i, j] = (int)Math.Sqrt(Math.Pow((Cordination[i].X - Cordination[j].X), 2) + Math.Pow((Cordination[i].Y - Cordination[j].Y), 2));
                }
            }
               
        }
        public int GetDistanceTwoPoints(int i, int j)
        {
            return Distance_Matrix[i-1, j-1];
        }

     }

    class Individual
    {
        #region properties
        public List<int> Chromosome { get; set; }
        public float Fitness { get; set; }
        public float Length { set; get; }    
        #endregion

        #region methods
        public void SetChromosome()
        {
            //randomly
            Chromosome = new List<int>();
            for (int i = 1; i < 128; i++)
                Chromosome.Add(i);
            new Random().Shuffle(Chromosome);

        }
        public void SetFitness()
        {
            Distance_Cities distances = new Distance_Cities();
       
            for (int i = 0; i < 126; i++)
                Length += distances.GetDistanceTwoPoints(Chromosome[i],Chromosome[i+1]);
            Fitness =(float) 100000 /(float)Length;
        }
        #endregion
    }

    class EvolutionaryAlgorithm
    {
        #region properties
        private int Size;
        private int EliteSize;
        public List<Individual> Population;
        public List<Individual> MatingPool = new List<Individual>();
        public List<Individual> OffSprings= new List<Individual>();
        public List<float> MyRouletteWheel=new List<float>();
        #endregion
        #region methods
        public EvolutionaryAlgorithm(int _size,int _elitesize)
        {
            Size = _size;
            EliteSize = _elitesize;
        }
        public void Initialization()
        {
            Population = new List<Individual>();
            for (int i = 0; i < Size; i++)
            {
                var ind = new Individual();
                ind.SetChromosome();
                ind.SetFitness();
                Population.Add(ind);
            }
        }
        public void SetRouletteWheel()
        {
            MyRouletteWheel.Clear();
            MyRouletteWheel = new List<float>() { 0};
            var sum = Population.Sum(o => o.Fitness);
            Population.ForEach(i =>
            MyRouletteWheel.Add(i.Fitness / sum+MyRouletteWheel.Last()));
        }
        public void ParentSelection()
        {
            var rng = new Random();
            MatingPool.Clear();
            for (int j=0;j<Size-EliteSize;j++)
            {
                int k = 0;
                var p = rng.NextDouble();
                while (p> MyRouletteWheel[k])
                {
                    k++;
                }
                MatingPool.Add(Population[k - 1]);
            }
            MatingPool.AddRange(Population.OrderByDescending(o => o.Fitness).Take(EliteSize));
        }
        public void SurvivorSelection()
        {
            new Random().Shuffle(OffSprings);
            Population = OffSprings;

        }
        public void Mutation(double MutationRate)
        {
            for (int i = 0; i < OffSprings.Count-EliteSize; i++)
            {
                if (MutationRate < new Random().NextDouble())
                {
                    int FirstIndex = new Random().Next(127);
                    int SecoundIndex = new Random().Next(127);
                    while (FirstIndex == SecoundIndex)
                    {
                        SecoundIndex = new Random().Next(127);
                    }

                    var temp = OffSprings[i].Chromosome[FirstIndex];
                    OffSprings[i].Chromosome[FirstIndex] = OffSprings[i].Chromosome[SecoundIndex];
                    OffSprings[i].Chromosome[SecoundIndex] = temp;
                    OffSprings[i].SetFitness();
                }
            }
        }
        public void Recombination()
        {
            OffSprings = new List<Individual>();
            for (int i = 0; i + 1 < MatingPool.Count-EliteSize; i = i + 2)
            {
                OffSprings.AddRange(CrossOver(MatingPool[i], MatingPool[i + 1]));
            }
            OffSprings.AddRange(MatingPool.Skip(MatingPool.Count - EliteSize));

        }
        public List<Individual> CrossOver(Individual A, Individual B)
        {
            Random rnd = new Random();
            Individual C = new Individual();
            Individual D = new Individual();    
            int P1 = rnd.Next(127);
            int P2 = P1;
            while (P1==P2)
                P2 = rnd.Next(127);
            int StartPoint= Math.Min(P1, P2);
            int EndPoint = Math.Max(P1, P2);
            C.Chromosome = A.Chromosome.Take(EndPoint).Skip(StartPoint).ToList();
            D.Chromosome = B.Chromosome.Take(EndPoint).Skip(StartPoint).ToList();
            for (int i = EndPoint; i < 127; i++)
            {
                if (!C.Chromosome.Contains(B.Chromosome[i]))                
                    C.Chromosome.Add(B.Chromosome[i]);
                
                if (!D.Chromosome.Contains(A.Chromosome[i]))               
                    D.Chromosome.Add(A.Chromosome[i]);              
            }
            for (int i = 0; i < EndPoint; i++)
            {
                if (!C.Chromosome.Contains(B.Chromosome[i]))                
                    C.Chromosome.Add(B.Chromosome[i]);
                
                if (!D.Chromosome.Contains(A.Chromosome[i]))               
                    D.Chromosome.Add(A.Chromosome[i]);            
            }
            C.Chromosome = C.Chromosome.Skip(A.Chromosome.Count - StartPoint)
                .Concat(C.Chromosome.Take(A.Chromosome.Count - StartPoint).ToList()).ToList();
            D.Chromosome = D.Chromosome.Skip(B.Chromosome.Count - StartPoint)
                .Concat(D.Chromosome.Take(B.Chromosome.Count - StartPoint).ToList()).ToList();
            C.SetFitness();
            D.SetFitness();
            return new List<Individual>() { C, D };
        }
        public void PrintMyPopulation()
        {
          
                Console.WriteLine("best fitness=" + Population.Max(i=>i.Fitness));
                Console.WriteLine("AVG fitness=" + Population.Average(i => i.Fitness));
                Console.WriteLine("best Length=" + Population.Min(i => i.Length));
                Console.WriteLine("AVG Length=" + Population.Average(i => i.Length));
            Console.WriteLine("best path =");
            foreach (var gen in Population.OrderBy(p => p.Fitness).First().Chromosome)
                Console.Write("," +gen);

        }
        #endregion
    }
    class Program
    {
        static void Main(string[] args)
        {           
            Distance_Cities ct = new Distance_Cities();
            ct.SetDistanceMatrix();
            Console.WriteLine("Initialization==> ");
            EvolutionaryAlgorithm MyEA = new EvolutionaryAlgorithm(1000,50);
            MyEA.Initialization();
            MyEA.PrintMyPopulation();
            int i = 1;
            for (; i<400; i++)
            {
                Console.WriteLine("\n Generation==> " + i);
                MyEA.SetRouletteWheel();
                MyEA.ParentSelection();
                MyEA.Recombination();
                MyEA.Mutation(0.8);
                MyEA.SurvivorSelection();
                MyEA.PrintMyPopulation();
            }
           
        }
    }
    static class RandomExtensions
    {
        public static void Shuffle<T>(this Random rng, List<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

    }
}
