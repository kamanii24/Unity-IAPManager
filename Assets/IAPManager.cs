// ========
// IAPManager.cs
// v1.0.0
// Created by Kamanii
// ========


#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// You must obfuscate your secrets using Window > Unity IAP > Receipt Validation Obfuscator
// before receipt validation will compile in this sample.
// #define RECEIPT_VALIDATION
#endif

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;
#if RECEIPT_VALIDATION
using UnityEngine.Purchasing.Security;
#endif

public class IAPManager : MonoBehaviour, IStoreListener {

	// 購入アイテムのデータ管理クラス
	[Serializable]
	public class PurchaseItemData {
		[SerializeField, Header("[Input Needed]")]
		// プロダクトID
		public string productId;
		// アイテム名
		public string itemName;
		// 消耗品・非消耗品・定期購入
		public ProductType productType;

		[SerializeField, Header("[Automatic Input]")]
		// アイテムの説明
		public string description;
		// コード
		public string currencyCode;
		// 価格
		public float price;
		// 価格文字列
		public string priceString;
	}

	// iOSセットアップ ================
	[SerializeField, Header("*iOS Store Setting")]
	public PurchaseItemData[] iosItems;

	// Androidセッティング ============
	[SerializeField, Header("*Android Store Setting")]
	// アイテムデータ
	public PurchaseItemData[] androidItems;
	// GooglePlay共通鍵文字列
	[SerializeField, TextArea()]
	public string googlePlayPublicKey = string.Empty;


	// デリゲート宣言
	public delegate void InitializedDelegate (PurchaseItemData[] items);
	public delegate void InitializeFailedDelegate (string error);
	public delegate void PurchaseDelegate (PurchaseItemData item);
	public delegate void RestoreDelegate ();

	// コールバックイベント
	public InitializedDelegate OnInitializedEvent;
	public InitializeFailedDelegate OnInitializeFailedEvent;
	public PurchaseDelegate OnPurchaseCompletedEvent;
	public PurchaseDelegate OnPurchaseFailedEvent;
	public RestoreDelegate OnRestoreCompletedEvent;


	// Unity IAP objects 
	private IStoreController storeController;
	private IAppleExtensions appleExtentions;

	// プロダクト返却
	public Product[] ItemProducts {
		get {
			return storeController.products.all;
		}
	}

	private int selectedItemIndex = -1; // -1 == no product
	private bool purchaseInProgress;

	private Selectable m_InteractableSelectable; // Optimization used for UI state management

	#if RECEIPT_VALIDATION
	private CrossPlatformValidator validator;
	#endif

	/// <summary>
	/// This will be called when Unity IAP has finished initialising.
	/// </summary>
	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		storeController = controller;
		appleExtentions = extensions.GetExtension<IAppleExtensions> ();

//		InitUI(controller.products.all);

		// On Apple platforms we need to handle deferred purchases caused by Apple's Ask to Buy feature.
		// On non-Apple platforms this will have no effect; OnDeferred will never be called.
		appleExtentions.RegisterPurchaseDeferredListener(OnDeferred);

		Debug.Log("Available items:");
		int index = 0;
		foreach (var item in controller.products.all)
		{
			if (item.availableToPurchase)
			{
				Debug.Log(string.Join(" - ",
					new[]
					{
						item.metadata.localizedTitle,
						item.metadata.localizedDescription,
						item.metadata.isoCurrencyCode,
						item.metadata.localizedPrice.ToString(),
						item.metadata.localizedPriceString
					}));

				#if UNITY_ANDROID
				androidItems[index].itemName		= item.metadata.localizedTitle;
				androidItems[index].description		= item.metadata.localizedDescription;
				androidItems[index].currencyCode	= item.metadata.isoCurrencyCode;
				androidItems[index].price			= (float)item.metadata.localizedPrice;
				androidItems[index].priceString		= item.metadata.localizedPriceString;
				#elif UNITY_IOS
				iosItems[index].itemName			= item.metadata.localizedTitle;
				iosItems[index].description			= item.metadata.localizedDescription;
				iosItems[index].currencyCode		= item.metadata.isoCurrencyCode;
				iosItems[index].price				= (float)item.metadata.localizedPrice;
				iosItems[index].priceString			= item.metadata.localizedPriceString;
				#endif

			}
			index++;
		}

		// Prepare model for purchasing
		if (storeController.products.all.Length > 0) {
			selectedItemIndex = -0;
		}


		// 初期化完了通知
		if (OnInitializedEvent != null) {
			Debug.Log ("初期化完了");
			#if UNITY_ANDROID
			OnInitializedEvent (androidItems);
			#elif UNITY_IOS
			OnInitializedEvent (iosItems);
			#endif
		}
	}

	/// <summary>
	/// This will be called when a purchase completes.
	/// </summary>
	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		Debug.Log("Purchase OK: " + e.purchasedProduct.definition.id);
		Debug.Log("Receipt: " + e.purchasedProduct.receipt);

		purchaseInProgress = false;

		#if RECEIPT_VALIDATION
		if (Application.platform == RuntimePlatform.Android ||
		Application.platform == RuntimePlatform.IPhonePlayer ||
		Application.platform == RuntimePlatform.OSXPlayer) {
		try {
		var result = validator.Validate(e.purchasedProduct.receipt);
		Debug.Log("Receipt is valid. Contents:");
		foreach (IPurchaseReceipt productReceipt in result) {
		Debug.Log(productReceipt.productID);
		Debug.Log(productReceipt.purchaseDate);
		Debug.Log(productReceipt.transactionID);

		GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
		if (null != google) {
		Debug.Log(google.purchaseState);
		Debug.Log(google.purchaseToken);
		}

		AppleInAppPurchaseReceipt apple = productReceipt as AppleInAppPurchaseReceipt;
		if (null != apple) {
		Debug.Log(apple.originalTransactionIdentifier);
		Debug.Log(apple.cancellationDate);
		Debug.Log(apple.quantity);
		}
		}
		} catch (IAPSecurityException) {
		Debug.Log("Invalid receipt, not unlocking content");
		return PurchaseProcessingResult.Complete;
		}
		}
		#endif


		// アイテムデータの取得
		#if UNITY_ANDROID
		PurchaseItemData[] items = androidItems;
		#elif UNITY_IOS
		PurchaseItemData[] items = iosItems;
		#endif

		Debug.Log(string.Join(" - ",
			new[]
			{
				e.purchasedProduct.definition.id,
				e.purchasedProduct.definition.storeSpecificId,
				e.purchasedProduct.metadata.localizedTitle,
				e.purchasedProduct.metadata.localizedDescription,
				e.purchasedProduct.metadata.isoCurrencyCode,
				e.purchasedProduct.metadata.localizedPrice.ToString(),
				e.purchasedProduct.metadata.localizedPriceString
			}));

		// プロダクトIDの検索
		foreach (PurchaseItemData item in items) {
			// 同じプロダクトIDであれば
			if (item.productId == e.purchasedProduct.definition.storeSpecificId) {
				// 購入完了通知
				if (OnPurchaseCompletedEvent != null)
					OnPurchaseCompletedEvent (item);
			}
		}

		// You should unlock the content here.

		// Indicate we have handled this purchase, we will not be informed of it again.x
		return PurchaseProcessingResult.Complete;
	}

	/// <summary>
	/// This will be called is an attempted purchase fails.
	/// </summary>
	public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
	{
		Debug.Log("Purchase failed: " + item.definition.id);
		Debug.Log(r);

		purchaseInProgress = false;


		// アイテムデータの取得
		#if UNITY_ANDROID
		PurchaseItemData[] items = androidItems;
		#elif UNITY_IOS
		PurchaseItemData[] items = iosItems;
		#endif

		// プロダクトIDの検索
		foreach (PurchaseItemData i in items) {
			// 同じプロダクトIDであれば
			if (i.productId == item.definition.storeSpecificId) {

				// 購入失敗通知
				if (OnPurchaseFailedEvent != null)
					OnPurchaseFailedEvent(i);
			}
		}

	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		Debug.Log("Billing failed to initialize!");
		switch (error)
		{
		case InitializationFailureReason.AppNotKnown:
			Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
			break;
		case InitializationFailureReason.PurchasingUnavailable:
			// Ask the user if billing is disabled in device settings.
			Debug.Log("Billing disabled!");
			break;
		case InitializationFailureReason.NoProductsAvailable:
			// Developer configuration error; check product metadata.
			Debug.Log("No products available for purchase!");
			break;
		}

		// 初期化失敗
		if (OnInitializeFailedEvent != null)
			OnInitializeFailedEvent ("Purchase Error");
	}

	public void Awake() {
		// 仮想ストアの設定
		var module = StandardPurchasingModule.Instance();
		module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
		var builder = ConfigurationBuilder.Instance(module);

		// iOSとAndroidの処理分岐
		#if UNITY_IOS
		string storeName = AppleAppStore.Name;
		PurchaseItemData[] itemData = iosItems;
		#elif UNITY_ANDROID
		string storeName = GooglePlay.Name;
		PurchaseItemData[] itemData = androidItems;
		// GooglePlay共通鍵の設定
		builder.Configure<IGooglePlayConfiguration>().SetPublicKey(googlePlayPublicKey);
		#endif

		// プロダクトの登録
		for (int i = 0; i < itemData.Length; i++) {
			// 各データの取得
			string name	= itemData [i].itemName;
			string pID	= itemData [i].productId;
			ProductType type = itemData [i].productType;

			// プロダクトの登録
			builder.AddProduct (name, type, new IDs {
				{ pID, storeName }
			});
		}

		#if RECEIPT_VALIDATION
		validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.bundleIdentifier);
		#endif

		// IAP初期化
		UnityPurchasing.Initialize(this, builder);
	}

	/// <summary>
	/// This will be called after a call to IAppleExtensions.RestoreTransactions().
	/// </summary>
	private void OnTransactionsRestored(bool success)
	{
		Debug.Log("Transactions restored.");

		// リストア完了通知
		if (OnRestoreCompletedEvent != null)
			OnRestoreCompletedEvent ();
	}

	/// <summary>
	/// iOS Specific.
	/// This is called as part of Apple's 'Ask to buy' functionality,
	/// when a purchase is requested by a minor and referred to a parent
	/// for approval.
	/// 
	/// When the purchase is approved or rejected, the normal purchase events
	/// will fire.
	/// </summary>
	/// <param name="item">Item.</param>
	private void OnDeferred(Product item)
	{
		Debug.Log("Purchase deferred: " + item.definition.id);
	}




	#region PUBLIC_ACCESS_METHOD
	// ==============
	// 外部からアクセスする必要のあるメソッド

	// アイテム購入処理
	public void Purchase (int index) {
		if (purchaseInProgress == true) {
			return;
		}

		storeController.InitiatePurchase(storeController.products.all[index]); 

		// Don't need to draw our UI whilst a purchase is in progress.
		// This is not a requirement for IAP Applications but makes the demo
		// scene tidier whilst the fake purchase dialog is showing.
		purchaseInProgress = true;
	}

	// 非消耗品アイテムのリストア処理
	public void Restore () {
		appleExtentions.RestoreTransactions(OnTransactionsRestored);
	}

	#endregion PUBLIC_ACCESS_METHOD
}
