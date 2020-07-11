
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace pokemon_type_calc
{
    class Program
    {
        static Dictionary<typeOrder, double[]> typeMatchups = new Dictionary<typeOrder, double[]>();
		static Dictionary<(typeOrder, typeOrder), int[]> typePairScores = new Dictionary<(typeOrder, typeOrder), int[]>();
		static List<(typeOrder, typeOrder, int[])> currentSuperEffectiveWinners = new List<(typeOrder, typeOrder, int[])>();
		static List<(typeOrder, typeOrder, int[])> currentSuperEffectiveLosers = new List<(typeOrder, typeOrder, int[])>();
		static int currentSuperEffectiveTargets = 0;
		static int currentLeastEffectiveTargets = 99;
		static List<(typeOrder, typeOrder, int[])> currentResistedWinners = new List<(typeOrder, typeOrder, int[])>();
		static List<(typeOrder, typeOrder, int[])> currentResistedLosers = new List<(typeOrder, typeOrder, int[])>();
		static int currentResistedTargets = 0;
		static List<(typeOrder, typeOrder, int[])> currentEffectiveDeltaWinners = new List<(typeOrder, typeOrder, int[])>();
		static int currentEffectiveDelta = 0;

		enum typeOrder { normal, fire, water, electric, grass, ice, fighting, poison, ground, flying, psychic, bug, rock, ghost, dragon, dark, steel, fairy };


        static void Main(string[] args)
        {
			//set up the matchup table
			typeMatchups.Add(typeOrder.normal, new double[18] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0.5, 0, 1, 1, 0.5, 1 });
			typeMatchups.Add(typeOrder.fire, new double[18] { 1, 0.5, 0.5, 1, 2, 2, 1, 1, 1, 1, 1, 2, 0.5, 1, 0.5, 1, 2, 1 });
			typeMatchups.Add(typeOrder.water, new double[18] { 1, 2, 0.5, 1, 0.5, 1, 1, 1, 2, 1, 1, 1, 2, 1, 0.5, 1, 1, 1 });
			typeMatchups.Add(typeOrder.electric, new double[18] { 1, 1, 2, 0.5, 0.5, 1, 1, 1, 0, 2, 1, 1, 1, 1, 0.5, 1, 1, 1 });
			typeMatchups.Add(typeOrder.grass, new double[18] { 1, 0.5, 2, 1, 0.5, 1, 1, 0.5, 2, 0.5, 1, 0.5, 2, 1, 0.5, 1, 0.5, 1});
			typeMatchups.Add(typeOrder.ice, new double[18] { 1, 0.5, 0.5, 1, 2, 0.5, 1, 1, 2, 2, 1, 1, 1, 1, 2, 1, 0.5, 1 });
			typeMatchups.Add(typeOrder.fighting, new double[18] { 2, 1, 1, 1, 1, 2, 1, 0.5, 1, 0.5, 0.5, 0.5, 2, 0, 1, 2, 2, 0.5 });
			typeMatchups.Add(typeOrder.poison, new double[18] { 1, 1, 1, 1, 2, 1, 1, 0.5, 0.5, 1, 1, 1, 0.5, 0.5, 1, 1, 0, 2 });
			typeMatchups.Add(typeOrder.ground, new double[18] { 1, 2, 1, 2, 0.5, 1, 1, 2, 1, 0, 1, 0.5, 2, 1, 1, 1, 2, 1 });
			typeMatchups.Add(typeOrder.flying, new double[18] { 1, 1, 1, 0.5, 2, 1, 2, 1, 1, 1, 1, 2, 0.5, 1, 1, 1, 0.5, 1 });
			typeMatchups.Add(typeOrder.psychic, new double[18] { 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 0.5, 1, 1, 1, 1, 0, 0.5, 1 });
			typeMatchups.Add(typeOrder.bug, new double[18] { 1, 0.5, 1, 1, 2, 1, 0.5, 0.5, 1, 0.5, 2, 1, 1, 0.5, 1, 2, 0.5, 0.5 });
			typeMatchups.Add(typeOrder.rock, new double[18] { 1, 2, 1, 1, 1, 2, 0.5, 1, 0.5, 2, 1, 2, 1, 1, 1, 1, 0.5, 1 });
			typeMatchups.Add(typeOrder.ghost, new double[18] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 1, 2, 1, 0.5, 0.5, 1 });
			typeMatchups.Add(typeOrder.dragon, new double[18] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1, 0.5, 0 });
			typeMatchups.Add(typeOrder.dark, new double[18] { 1, 1, 1, 1, 1, 1, 0.5, 1, 1, 1, 2, 1, 1, 2, 1, 0.5, 0.5, 0.5 });
			typeMatchups.Add(typeOrder.steel, new double[18] { 1, 0.5, 0.5, 0.5, 1, 2, 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 0.5, 2 });
			typeMatchups.Add(typeOrder.fairy, new double[18] { 1, 0.5, 1, 1, 1, 1, 2, 0.5, 1, 1, 1, 1, 1, 1, 2, 2, 0.5, 1 });

			//calculate type effects
			foreach (var type in Enum.GetNames(typeof(typeOrder)))
			{
				//primary type
				typeOrder currentType;
				Enum.TryParse(type, out currentType);


				
				foreach (var oType in Enum.GetNames(typeof(typeOrder)))
				{
					//secondary type
					int superTargets = 0;
					int resistTargets = 0;
					int superHits = 0;
					int resistHits = 0;
					int immuneHits = 0;
					typeOrder currentSecondaryType;
					Enum.TryParse(oType, out currentSecondaryType);

					//skip mirror types
					if (!((int)currentType >= (int)currentSecondaryType))
					{
						//calculate offense for primary type
						foreach (var item in typeMatchups[currentType])
						{
							if (item == 0.5)
							{
								resistTargets++;
							}
							else if (item == 2)
							{
								superTargets++;
							}
						}

						//calculate offense for secondary type
						foreach (var item in typeMatchups[currentSecondaryType])
						{
							if (item == 0.5)
							{
								resistTargets++;
							}
							if (item == 2)
							{
								superTargets++;
							}
						}

						//calculate defensiveness
						foreach (var item in typeMatchups)
						{
							double effect = (item.Value[(int)currentType] * item.Value[(int)currentSecondaryType]);
							if (effect == 0)
							{
								immuneHits++;
							}
							else if (effect < 1)
							{
								resistHits++;
							}
							else if (effect > 1)
							{
								superHits++;
							}

						}
						typePairScores.Add((currentType, currentSecondaryType), new int[5] { superTargets, resistTargets, superHits, resistHits, immuneHits });
					}
				}
			}

			//add calculations to relevant result lists
			foreach (var item in typePairScores)
			{
				currentSuperEffectiveWinners.Add((item.Key.Item1, item.Key.Item2, item.Value));
				currentSuperEffectiveLosers.Add((item.Key.Item1, item.Key.Item2, item.Value));
				currentResistedWinners.Add((item.Key.Item1, item.Key.Item2, item.Value));
				currentResistedLosers.Add((item.Key.Item1, item.Key.Item2, item.Value));
			}

			//sort and preen the result lists
			currentSuperEffectiveWinners.Sort(sortOffense);
			while (currentSuperEffectiveWinners.Count > 10)
			{
				currentSuperEffectiveWinners.Remove(currentSuperEffectiveWinners[0]);
			}

			currentSuperEffectiveLosers.Sort(sortOffense);
			while (currentSuperEffectiveLosers.Count > 10)
			{
				currentSuperEffectiveLosers.Remove(currentSuperEffectiveLosers[currentSuperEffectiveLosers.Count-1]);
			}

			currentResistedWinners.Sort(sortDefense);
			while (currentResistedWinners.Count > 10)
			{
				currentResistedWinners.Remove(currentResistedWinners[0]);
			}

			currentResistedLosers.Sort(sortDefense);
			while (currentResistedLosers.Count > 10)
			{
				currentResistedLosers.Remove(currentResistedLosers[currentResistedLosers.Count - 1]);
			}

			int sortOffense ((typeOrder, typeOrder, int[]) x, (typeOrder, typeOrder, int[]) y)
			{
				return x.Item3[0].CompareTo(y.Item3[0]);
			}

			int sortDefense((typeOrder, typeOrder, int[]) x, (typeOrder, typeOrder, int[]) y)
			{
				return x.Item3[2].CompareTo(y.Item3[2]);
			}


			//print results
			Console.WriteLine("The most super effecive type combinations:");
			foreach (var item in currentSuperEffectiveWinners)
			{
				Console.WriteLine($"{item.Item1}/{item.Item2} with {item.Item3[0]}");
			}

			Console.WriteLine("\nThe 10 least effective type combinations:");
			foreach (var item in currentSuperEffectiveLosers)
			{
				Console.WriteLine($"{item.Item1}/{item.Item2} with {item.Item3[0]}");
			}

			Console.WriteLine("\nThe 10 least resistant type combinations:");
			foreach (var item in currentResistedLosers)
			{
				Console.WriteLine($"{item.Item1}/{item.Item2} with {item.Item3[2]}");
			}

			Console.WriteLine("\nThe 10 most resistant type combinations:");
			foreach (var item in currentResistedWinners)
			{
				Console.WriteLine($"{item.Item1}/{item.Item2} with {item.Item3[2]}");
			}
		} 
    }
}
