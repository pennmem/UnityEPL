using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordListGenerator : MonoBehaviour 
{
	public string[] unshuffled_words;

	public string[,] GenerateLists(int randomSeed, int numberOfLists, int lengthOfEachList)
	{
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