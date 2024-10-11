using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleGameLanguageSelector : MonoBehaviour
{
	[Header ("Main Settings")]
	[Space]

	public gameLanguageSelector mainGameLanguageSelector;

	[Space]
	[Header ("Debug")]
	[Space]

	public bool gameLanguageSelectorLocated;


	public void setGameLanguage (string languageSelected)
	{
		checkGetMainGameLanguageSelector ();

		if (gameLanguageSelectorLocated) {
			mainGameLanguageSelector.setGameLanguage (languageSelected);
		}
	}

	void checkGetMainGameLanguageSelector ()
	{
		if (!gameLanguageSelectorLocated) {
			gameLanguageSelectorLocated = mainGameLanguageSelector != null;

			if (!gameLanguageSelectorLocated) {
				mainGameLanguageSelector = gameLanguageSelector.Instance;

				gameLanguageSelectorLocated = mainGameLanguageSelector != null;
			}

			if (!gameLanguageSelectorLocated) {
				GKC_Utils.instantiateMainManagerOnSceneWithTypeOnApplicationPlaying (gameLanguageSelector.getMainManagerName (), typeof(gameLanguageSelector), true);

				mainGameLanguageSelector = gameLanguageSelector.Instance;

				gameLanguageSelectorLocated = (mainGameLanguageSelector != null);
			}

			if (!gameLanguageSelectorLocated) {
				mainGameLanguageSelector = FindObjectOfType<gameLanguageSelector> ();

				gameLanguageSelectorLocated = mainGameLanguageSelector != null;
			} 
		}
	}
}
