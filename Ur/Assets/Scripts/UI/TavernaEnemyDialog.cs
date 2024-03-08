using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MiniGameFramework;

public class TavernaEnemyDialog : TavernaMiniGameDialog
{
	private const string DefaultPortrait = "crew_portraits/phoenician_sailor";

	private CrewMember crew;

    protected override void SetDisplayInformation()
    {
        if (crew == null) {
            crew = GameManager.MasterCrewList.RandomElement();
        }

        // TODO: Hook to kylie's better dialog system
        //nameText.text = crew.CrewName;
        //portrait.sprite = crew.CrewPortrait;

        //if (portrait.sprite == null) {
        //    portrait.sprite = Resources.Load<Sprite>(DefaultPortrait);
        //}
    }

	public override void ShowCharacterInfo() {
		base.ShowCharacterInfo();
		dialog.text = crew.CrewDescription;
	}
}
