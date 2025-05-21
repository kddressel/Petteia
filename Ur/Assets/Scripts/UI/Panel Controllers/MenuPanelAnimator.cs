using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanelAnimator : MonoBehaviour
{
	public bool activateOnEnable = false;
	public Animator anim;
	public float toOffAnimTime;
	public Scrollbar scroll;

	private void OnEnable() {
		if (activateOnEnable) {
			EnableAnimation(true);
		}
	}

	public void EnableAnimation(bool enable)
    {
        gameObject.SetActive(true);
        if (anim != null)
		{
            if (enable)
            {
                StartCoroutine(ResetScroll());
            }
            anim.SetBool("MenuActive", enable);
        }
	}

	public void CloseMenu() {
		if(gameObject != null && gameObject.activeInHierarchy)
		{
            StartCoroutine(DoCloseMenu());
        }
    }

	public IEnumerator DoCloseMenu() {
		if(anim == null)
        {
            gameObject.SetActive(false);
        }
		else
        {
            anim.SetBool("MenuActive", false);
            yield return new WaitForSecondsRealtime(toOffAnimTime);
            gameObject.SetActive(false);
        }
	}

	private IEnumerator ResetScroll() {
		yield return null;
		yield return null;
		if(scroll != null) {
			scroll.value = 1f;
		}
	}
}
