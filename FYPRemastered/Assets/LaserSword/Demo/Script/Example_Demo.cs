using UnityEngine;
using UnityEngine.UI;


namespace ArmnomadsGames
{

	public class Example_Demo : MonoBehaviour
	{

		[SerializeField()]
		Button show_btn;
		[SerializeField()]
		Button hide_btn;
		[SerializeField()]
		Button character_hit_btn;
		[SerializeField()]
		Button character_idle_btn;

		[Space(20)]

		[SerializeField()]
		LaserSword LaserSword;
		[SerializeField()]
		Animator character_animator;

		[Space(20)]

		[SerializeField()]
		Camera camera_1;
		[SerializeField()]
		Camera camera_2;




		// Use this for initialization
		void Start()
		{
			show_btn.onClick.AddListener(ShowLightsaber);
			hide_btn.onClick.AddListener(HideLightsaber);
			character_idle_btn.onClick.AddListener(SetIdle);
			character_hit_btn.onClick.AddListener(SetHit);
		}




		void ShowLightsaber()
		{
			LaserSword.Enable();
		}

		void HideLightsaber()
		{
			LaserSword.Disable();
		}

		void SetIdle()
		{
			character_animator.SetTrigger("idle");
			camera_1.gameObject.SetActive(true);
			camera_2.gameObject.SetActive(false);
		}

		void SetHit()
		{
			character_animator.SetTrigger("hit");
			camera_1.gameObject.SetActive(false);
			camera_2.gameObject.SetActive(true);
		}

	}

}