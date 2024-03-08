using UnityEngine;

public class DifficultySelectorButton : MonoBehaviour
{
	public AIDifficulty difficulty;

	public void SetDifficulty() {
		if (GameManager.SelectedDifficulty != difficulty) {
			GameManager.SelectedDifficulty = difficulty;
		}
	}
}
