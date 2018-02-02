# Unity IAPManager
UnityIAPを個人的に使いやすくまとめたものです。<br>
iOS/Androidのみ対応です。

#### 自プロジェクトでの使用
同封の**Unity-IAPManager.unitypackage**をそのままインポートするだけです。<br>
IAPManagerを使用するために*Window>Service*からIn-App Purchasingを有効にし、UnityIAPパッケージをインポートしてください。<br>

# 使い方

#### アイテム情報の登録
IAPManagerを空のGameObjectにアタッチし、Inspectorで各ストアアイテムの情報を登録します。<br>
iOSとAndroid二つの項目が用意されているのでそれぞれ入力します。<br>
![Imgur](https://i.imgur.com/frU9CWR.png)
<br>

#### イベントハンドラの設定
IAPManagerの各処理のイベントを受け取るためにイベントハンドラを設定します。

      public IAPManager iapManager;
      void Awake () {
         iapManager.OnInitializedEvent = OnInitialized;
         iapManager.OnInitializeFailedEvent = OnInitializeFailed;
         iapManager.OnPurchaseCompletedEvent = OnPurchaseCompleted;
         iapManager.OnPurchaseFailedEvent = OnPurchaseFailed;
         iapManager.OnRestoreCompletedEvent	= OnRestoreCompleted;
      }
      
      // 初期化完了
      private void OnInitialized(IAPManager.PurchaseItemData[] items) {
         // 初期化完了処理
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


#### 購入処理
IAPManagerクラスのPurchaseクラスにアイテムのindexを引数として渡します。

      // アイテム購入ボタンが押された
      public void OnTapPurchaseButton (int itemIndex) {
          // 購入
          iapManager.Purchase (itemIndex);
      }


#### リストア(iOSのみ)
購入済みのアイテムを復元します。<br>
復元されたアイテム情報は*OnPurchaseCompletedEvent*にて受け取ります。

      // アイテム購入ボタンが押された
      public void OnTapRestoreButton () {
          // リストア
          iapManager.Restore ();
      }


## ビルド環境
Unity 2017.3.0p3<br>
macOS High Sierra 10.13.3
