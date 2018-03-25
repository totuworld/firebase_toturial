using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Unity.Editor;
using Facebook.Unity;

public class gfb_auth_test : MonoBehaviour {

  /** auth 용 instance */
  FirebaseAuth auth;
  /** 사용자 */
  FirebaseUser user;

  string displayName;
  string emailAddress;
  string photoUrl;

  /** 상태 출력용 */
  public Text txtPrint;

  void Start()
  {
    // 초기화
    InitializeFirebase();

    // facebook sdk 초기화
    if (!FB.IsInitialized) {
      FB.Init(FacebookInitCallBack, OnHideUnity);
    }
  }

  void InitializeFirebase() {
    auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    auth.StateChanged += AuthStateChanged;
  }

  /** 상태변화 추적 */
  void AuthStateChanged(object sender, System.EventArgs eventArgs) {
    if (auth.CurrentUser != user) {
      bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
      if (!signedIn && user != null) {
        Debug.LogFormat("Signed out {0}", user.UserId);
      }
      user = auth.CurrentUser;
      if (signedIn) {
        Log(string.Format("Signed in {0}", user.UserId));
        displayName = user.DisplayName ?? "";
        emailAddress = user.Email ?? "";
        Log(string.Format("Signed in {0} _ {1}", displayName, emailAddress));
      }
    }
  }

  /** 익명 로그인 요청 */
  public void anoymousLogin() {
    auth
      .SignInAnonymouslyAsync()
      .ContinueWith(task => {
        if (task.IsCanceled) {
          Debug.LogError("SignInAnonymouslyAsync was canceled.");
          return;
        }
        if (task.IsFaulted) {
          Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
          return;
        }

        user = task.Result;
        Log(string.Format("User signed in successfully: {0} ({1})",
            user.DisplayName, user.UserId));
      });
  }

  void Log(string logText)
  {
    txtPrint.text += (logText + "\n");
    Debug.Log(logText);
  }

  /** Facebook 초기화 콜백 */
  void FacebookInitCallBack() {
    if (FB.IsInitialized) {
      FB.ActivateApp();
    } else {
      Log("Failed to Initalize the Facebook SDK");
    }
  }

  /** Facebook 로그인이 활성화되는 경우 호출 */
  void OnHideUnity(bool isGameShown) {
    if (!isGameShown) {
      // 게임 일시 중지
      Time.timeScale = 0;
    } else {
      // 게임 재시작
      Time.timeScale = 1;
    }
  }

  /** 페이스북 로그인 요청(버튼과 연결) */
  public void facebookLogin() {
    var param = new List<string>() { "public_profile", "email" };
    FB.LogInWithReadPermissions(param);
  }

  /** 페이스북 로그인 결과 콜백 */
  void FacebookAuthCallback(ILoginResult result) {
    if (result.Error != null) {
      Log(string.Format("Facebook Auth Error: {0}", result.Error));
      return;
    }
    if (FB.IsLoggedIn) {
      var accessToken = AccessToken.CurrentAccessToken;
      Log(string.Format("Facebook access token: {0}", accessToken.TokenString));

      // TODO: firebase facebook 로그인 연결 호출 부분

    } else {
      Log("User cancelled login");
    }
  }
}
