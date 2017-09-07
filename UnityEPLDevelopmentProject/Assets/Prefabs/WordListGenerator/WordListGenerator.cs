using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WordListGenerator
{
	public string[] unshuffled_words = new string[]
	{
		"ANT", "APE", "ARK", "ARM", "AXE", "BADGE", "BAG", "BALL", "BAND", "BANK", "BARN", "BAT", "BATH", "BEACH", "BEAK", "BEAN", "BEAR", "BED", "BEE", "BENCH", "BIRD", "BLOOM", "BLUSH", "BOARD", "BOAT", "BOMB", "BOOK", "BOOT", "BOWL", "BOX", "BOY", "BRANCH", "BREAD", "BRICK", "BRIDGE", "BROOM", "BRUSH", "BUSH", "CAGE", "CAKE", "CALF", "CANE", "CAPE", "CAR", "CART", "CASH", "CAT", "CAVE", "CHAIR", "CHALK", "CHEEK", "CHIEF", "CHIN", "CLAY", "CLIFF", "CLOCK", "CLOTH", "CLOUD", "CLOWN", "COIN", "CONE", "CORD", "CORN", "COUCH", "COW", "CRANE", "CROW", "CROWN", "CUBE", "CUP", "DART", "DEER", "DESK", "DIME", "DITCH", "DOCK", "DOG", "DOLL", "DOOR", "DRESS", "DRUM", "DUCK", "EAR", "EEL", "EGG", "ELF", "FACE", "FAN", "FARM", "FENCE", "FILM", "FISH", "FLAG", "FLAME", "FLEA", "FLOOR", "FLUTE", "FOAM", "FOG", "FOOD", "FOOT", "FORK", "FORT", "FOX", "FROG", "FRUIT", "FUDGE", "FUR", "GATE", "GEESE", "GLASS", "GLOVE", "GOAT", "GOLD", "GRAPE", "GRASS", "GUARD", "HAND", "HAT", "HAWK", "HEART", "HEN", "HILL", "HOLE", "HOOF", "HOOK", "HORN", "HORSE", "HOSE", "HOUSE", "ICE", "INK", "JAIL", "JAR", "JEEP", "JET", "JUDGE", "JUICE", "KEY", "KITE", "LAKE", "LAMB", "LAMP", "LAND", "LAWN", "LEAF", "LEG", "LIP", "LOCK", "MAIL", "MAP", "MAT", "MAZE", "MILK", "MOLE", "MOON", "MOOSE", "MOTH", "MOUSE", "MOUTH", "MUD", "MUG", "MULE", "NAIL", "NEST", "NET", "NOSE", "OAK", "OAR", "OWL", "PALM", "PANTS", "PARK", "PASTE", "PEA", "PEACH", "PEAR", "PEARL", "PEN", "PET", "PHONE", "PIE", "PIG", "PIN", "PIPE", "PIT", "PLANE", "PLANT", "PLATE", "POLE", "POND", "POOL", "PRINCE", "PURSE", "RAIN", "RAKE", "RAT", "RIB", "RICE", "ROAD", "ROCK", "ROOF", "ROOM", "ROOT", "ROPE", "ROSE", "RUG", "SAIL", "SALT", "SCHOOL", "SEA", "SEAL", "SEAT", "SEED", "SHARK", "SHEEP", "SHEET", "SHELL", "SHIELD", "SHIP", "SHIRT", "SHOE", "SHRIMP", "SIGN", "SINK", "SKI", "SKUNK", "SKY", "SLEEVE", "SLIME", "SLUSH", "SMILE", "SMOKE", "SNAIL", "SNAKE", "SNOW", "SOAP", "SOCK", "SOUP", "SPARK", "SPEAR", "SPONGE", "SPOON", "SPRING", "SQUARE", "STAIR", "STAR", "STEAK", "STEAM", "STEM", "STICK", "STONE", "STOOL", "STORE", "STORM", "STOVE", "STRAW", "STREET", "STRING", "SUIT", "SUN", "SWAMP", "SWORD", "TAIL", "TANK", "TEA", "TEETH", "TENT", "THREAD", "THUMB", "TIE", "TOAD", "TOAST", "TOE", "TOOL", "TOOTH", "TOY", "TRAIN", "TRASH", "TRAY", "TREE", "TRUCK", "VAN", "VASE", "VEST", "VINE", "WALL", "WAND", "WAVE", "WEB", "WEED", "WHALE", "WHEEL", "WING", "WOLF", "WOOD", "WORLD", "WORM", "YARD", "ZOO"
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