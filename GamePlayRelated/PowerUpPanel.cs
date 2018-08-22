using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpPanel : MonoBehaviour, AdStatusCallback {

    public string selectedPowerUp = "";
    public Image powerUpIcon;
    public Image powerUpIconPlayer;
    public Image powerUpIconPlayer2;
    public Text powerUpNameShadow;
    public Text powerUpName;
    private IronSourceAdManager adManager;
    public Sprite[] iconSprites;
    public Sprite[] playerSprites;
    public Text descriptionText;
    //private Atlas hammer2ShopItemsAtlas;


    // Use this for initialization
    void Start () {
        adManager = FindObjectOfType<IronSourceAdManager>();
        //hammer2ShopItemsAtlas = AtlasManager.hammer2ShopItemsAtlas;
        SelectPowerUp();
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RentPowerUp()
    {
        adManager.ShowRewardedAd(this, "PowerUp", "Level" + GameData.mission);
    }

    public void SelectPowerUp()
    {
        
        int index = Random.Range(1, 12);
        if (index >= 1 && index < 5)
        {
            index = 1;
        }else if (index >= 5 && index < 9)
        {
            index = 8;
        }else if (index >= 9 && index < 13)
        {
            index = 9;
        }
        while (ShopItemManager.IsBought("ShopItem" + index) || ShopItemManager.IsRented("ShopItem" + index) || index == 5)
        {
            index = Random.Range(1, 13);
            if (index >= 1 && index < 5)
            {
                index = 1;
            }
            else if (index >= 5 && index < 9)
            {
                index = 8;
            }
            else if (index >= 9 && index < 13)
            {
                index = 9;
            }
        }
        selectedPowerUp = "ShopItem" + index;
        SetPowerUpIcon(index);
    }

    public void SetPowerUpIcon(int index)
    {
       switch(index)
        {
            case 1:
                powerUpIcon.sprite = iconSprites[0];
                //powerUpIconPlayer.sprite = playerSprites[0];
                //powerUpIconPlayer2.sprite = playerSprites[1];
                descriptionText.text = "• Fly through the air!\n• Snipe from above!\n• Keep until you die!";
                break;

            case 8:
                powerUpIcon.sprite = iconSprites[2];
                //powerUpIconPlayer.sprite = playerSprites[3];
                //powerUpIconPlayer2.sprite = playerSprites[3];
                descriptionText.text = "• Find loots easier!\n• Get more loots!\n• Keep until you die!";
                break;

            case 9:
                powerUpIcon.sprite = iconSprites[1];
                //powerUpIconPlayer.sprite = playerSprites[2];
                //powerUpIconPlayer2.sprite = playerSprites[2];
                descriptionText.text = "• Get twice the blocks!\n• Get More power ups!\n• Keep until you die!";
                break;
        }
        powerUpNameShadow.text = "Amazing " + XLocalization.Get("ShopItem" + index + "HeaderText");
        powerUpName.text = "Amazing " + XLocalization.Get("ShopItem" + index + "HeaderText");
    }

    #region AdStatusCallback implementation

    public void onVideoAdWatched(VideoResult result)
    {
        if (result == VideoResult.RewardPlayer)
        {
            RewardFunction();
        }
    }

    void RewardFunction()
    {
        //Reward Function Code
        ShopItemManager.Rent(selectedPowerUp);
    }

    #endregion
}
