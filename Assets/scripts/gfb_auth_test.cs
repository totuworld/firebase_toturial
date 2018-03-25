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

    /** firebase 앱 내에 가입 여부를 체크한다. */
  private bool SingedInFirebase {
    get {
      return user != auth.CurrentUser && auth.CurrentUser != null;
    }
  }

  /** 상태변화 추적 */
  void AuthStateChanged(object sender, System.EventArgs eventArgs) {
    if (auth.CurrentUser != user) {

      if (!SingedInFirebase && user != null) {
        Debug.LogFormat("Signed out {0}", user.UserId);
      }
      user = auth.CurrentUser;
      if (SingedInFirebase) {
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

  #region FACEBOOK 로그인
  /** Facebook 초기화 콜백 */
  void FacebookInitCallBack() {
    if (FB.IsInitialized) {
      Log("Successed to Initalize the Facebook SDK");
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
    FB.LogInWithReadPermissions(param, FacebookAuthCallback);
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

      // 이미 firebase에 account 등록이 되었는지 확인
      if (SingedInFirebase) {
        linkFacebookAccount(accessToken);
      } else {
        // firebase facebook 로그인 연결 호출 부분
        registerFacebookAccountToFirebase(accessToken);
      }

    } else {
      Log("User cancelled login");
    }
  }

    /** Facebook access token으로 Firebase 등록 요청 */
  void registerFacebookAccountToFirebase(AccessToken accessToken) {
    Credential credential = FacebookAuthProvider.GetCredential(accessToken.TokenString);

    auth
      .SignInWithCredentialAsync(credential)
      .ContinueWith(task => {
        if (task.IsCanceled) {
          Log("SignInWithCredentialAsync was canceled.");
          return;
        }
        if (task.IsFaulted) {
          Log("SignInWithCredentialAsync encountered an error: " + task.Exception);
          return;
        }

        user = task.Result;
        Log(string.Format("User signed in successfully: {0} ({1})",
            user.DisplayName, user.UserId));
      });
  }

  /** Firebase에 등록된 account를 보유했을 때 새로운 인증을 연결한다. */
  void linkFacebookAccount(AccessToken accessToken) {
    Credential credential = FacebookAuthProvider.GetCredential(accessToken.TokenString);

    auth.CurrentUser
      .LinkWithCredentialAsync(credential)
      .ContinueWith(task => {
        if (task.IsCanceled) {
          Log("LinkWithCredentialAsync was canceled.");
          return;
        }
        if (task.IsFaulted) {
          Log("LinkWithCredentialAsync encountered an error: " + task.Exception);
          return;
        }

        user = task.Result;
        Log(string.Format("Credentials successfully linked to Firebase user: {0} ({1})",
            user.DisplayName, user.UserId));
      });
  }
  #endregion
}
