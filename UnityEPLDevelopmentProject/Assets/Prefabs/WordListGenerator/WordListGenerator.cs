using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WordListGenerator
{
	public string[] unshuffled_words = new string[]
	{
		"Apple",
		"Cat",
		"Russia",
		"Henceforth"
	};

	public abstract string UsedInExperiment();

	public abstract string[,] GenerateLists (int randomSeed, int numberOfLists, int lengthOfEachList);

	public string[] ShuffledWords(System.Random random)
	{
		string[] shuffled_words = (string[])unshuffled_words.Clone();
		for (int swaper = 0; swaper < shuffled_words.Length; swaper++)
		{
			int swapee = random.Next(swaper, shuffled_words.Length);
			string swapee_word = shuffled_words[swapee];
			shuffled_words[swapee] = shuffled_words[swaper];
			shuffled_words[swaper] = swapee_word;  
		}
		return shuffled_words;
	}
}

public class FR1ListGenerator : WordListGenerator
{
	public override string UsedInExperiment()
	{
		return "FR1";
	}

	public override string[,] GenerateLists (int randomSeed, int numberOfLists, int lengthOfEachList)
	{
		Debug.Log ((numberOfLists * lengthOfEachList) > unshuffled_words.Length);
		if ((numberOfLists * lengthOfEachList) > unshuffled_words.Length)
			throw new UnityException("There aren't enough words for those parameters");

		string[,] lists = new string[numberOfLists, lengthOfEachList];

		System.Random random = new System.Random (randomSeed);
		ShuffledWords (random);
		string[] shuffled_words = ShuffledWords (random);

		for (int i = 0; i < numberOfLists; i++)
		{
			for (int j = 0; j < lengthOfEachList; j++)
			{
				lists [i, j] = shuffled_words [i * lengthOfEachList + j];
			}
		}

		return lists;
	}
}