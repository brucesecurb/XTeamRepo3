using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RateUsPanel : MonoBehaviour {

    public Image[] starSprites;
    //public Sprite yellowStarSprite;
    private int starRank;
    public int BVC;
    public string email;

    // Use this for initialization
    void Start() {
        
        //yellowStarSprite = starSprites[0].GetComponent<Button>().
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void SetStar(int star)
    {
        starRank = star;
        LoadStars();
        PlayerPrefs.SetInt("Stars", starRank);
        RateApp();
        Scripts.interfaceScript.OnButton("DontAskButton");
    }

    public void LoadStars()
    {
        for(int i = 0; i < starRank; i++)
        {
            starSprites[i].color = Color.white;
        }
    }

    private void SendBadEmail()
    {
        
        string subject = MyEscapeURL("Customer Feedback");
        string body = MyEscapeURL("Tell us what you think about our game, send suggestions and ideas! And feel free to report bugs!\n" + "BundleID " + Application.identifier + " " + Application.version + ", BVC " + BVC);

        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }

    private string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }

    public void RateApp()
    {
        if(starRank <= 3)
        {
            SendBadEmail();
        }else
        {
#if UNITY_ANDROID
            Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
#endif
#if UNITY_IOS
			Application.OpenURL("https://itunes.apple.com/us/app/"+Application.productName.ToLower().Replace(" ","-")+"/id"+ IOSAppSettings.Instance.iOSStoreID);
#endif
        }
    }

}
