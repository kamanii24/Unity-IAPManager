using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PurchaseController : MonoBehaviour {

	// public
	public IAPManager iapManager;

	public Text[] itemNames;
	public Text[] prices;

	#region INITIALIZE
	// Use this for initialization
	void Awake () {
		// コールバックイベント登録
		iapManager.OnInitializedEvent		= OnInitialized;
		iapManager.OnInitializeFailedEvent	= OnInitializeFailed;
		iapManager.OnPurchaseCompletedEvent = OnPurchaseCompleted;
		iapManager.OnPurchaseFailedEvent	= OnPurchaseFailed;
		iapManager.OnRestoreCompletedEvent	= OnRestoreCompleted;
	}
	#endregion


	// 購入購入
	public void Purchase(int itemIndex) {
		// 購入処理
		iapManager.Purchase (itemIndex);
	}

	public void Restore() {
		// 購入処理
		iapManager.Restore ();
	}


	#region PURCHASE_EVENT_HANDLER

	// 初期化完了
	private void OnInitialized(IAPManager.PurchaseItemData[] items) {
		// 初期化完了処理
		int index = 0;
		foreach (IAPManager.PurchaseItemData item in items) {
			// アイテム情報の設定
			itemNames[index].text = item.itemName;
			prices[index].text = item.priceString;
			index++;
		}
	}

	// 初期化失敗
	private void OnInitializeFailed(string error) {
		// エラー処理

	}


	// 購入完了
	private void OnPurchaseCompleted(IAPManager.PurchaseItemData itemData) {
		// 購入完了処理

	}


	// 購入失敗
	private void OnPurchaseFailed(IAPManager.PurchaseItemData item) {
		// 購入失敗処理

	}

	// リストア完了
	private void OnRestoreCompleted () {
		// リストア完了処理

	}

	#endregion PURCHASE_EVENT_HANDLER
}
