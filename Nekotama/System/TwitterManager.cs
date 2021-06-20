using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using TwitterKit.Unity;
using TwitterKit.Unity.Settings;
using Twity.DataModels.Core;
using Twity.DataModels.Errors;

public class TwitterManager : MonoBehaviour {

    private static TwitterManager twitterManager = null;
    private GameObject twitterObj = null;

    private string tweetMassage;
    private string tweetMedia;

    private static StringBuilder m_builder = new StringBuilder();

    private bool isRetry;   //セッションが切れていてツイートをリトライしているかどうか

    private DelFuncs delFuncs;

    //ツイート結果を受け取るためのコールバック関数を格納するクラス
    public class DelFuncs {
        public delegate void CallbackSuccess();             //ツイート成功時
        public delegate void CallbackFaied(string errMsg);  //ツイート失敗時、エラー内容をerrMsgで受け取る
        public CallbackSuccess callbackSuccess { get; }
        public CallbackFaied callbackFailed { get; }


        public DelFuncs(CallbackSuccess callSuccess, CallbackFaied callFailed) {
            callbackSuccess = callSuccess;
            callbackFailed = callFailed;
        }
    }


    public static void TweetSet(DelFuncs delFunc, string msg, string mediaUrl = null) {

        Twitter.Init();

        Getinstance().LocalInit(msg, mediaUrl, delFunc);

    }


    private static TwitterManager Getinstance() {

        if (twitterManager == null) {
            var tmpObj = new GameObject("objTwitter");
            twitterManager = tmpObj.AddComponent<TwitterManager>();
            twitterManager.InitObj();
        }

        return twitterManager;
    }

    private void InitObj() {
        if (twitterObj == null) {
            twitterObj = gameObject;
            GameObject.DontDestroyOnLoad(twitterObj);
        }
    }


    private void LocalInit(string msg, string media, DelFuncs delfunc) {

        tweetMassage = msg;
        tweetMedia = media;
        delFuncs = delfunc;
        isRetry = false;

        TwitterSession twitterSession = Twitter.Session;

        if (twitterSession == null) {
            LocalLogin();

        } else if (twitterSession.id <= 0) {
            LocalLogin();

        } else {
            LocalTweetMake(twitterSession);

        }

    }

    //ログイン
    private void LocalLogin() {

        Twitter.LogIn(LocalOnLogin, error => m_builder.AppendLine(error.message));
        return;


    }

    //ログイン成功時にツイートする
    private void LocalOnLogin(TwitterSession session) {

        LocalTweetMake(session);


    }

    private void SessionRetry() {

        Twitter.LogOut();

        isRetry = true;

        LocalLogin();


    }

    //ツイートを作る
    private void LocalTweetMake(TwitterSession session) {

        if (tweetMedia != null) {
            LocalTweetMakeWithMedia(session);
            return;
        }
        var auth = session.authToken;
        var accessToken = auth.token;
        var accessTokenSecret = auth.secret;

        Twity.Oauth.consumerKey = TwitterSettings.ConsumerKey;
        Twity.Oauth.consumerSecret = TwitterSettings.ConsumerSecret;
        Twity.Oauth.accessToken = accessToken;
        Twity.Oauth.accessTokenSecret = accessTokenSecret;


        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters["status"] = tweetMassage;
        StartCoroutine(Twity.Client.Post("statuses/update", parameters, CallbackTweet));

    }


    void CallbackTweet(bool success, string response) {
        if (success) {
            Tweet tweet = JsonUtility.FromJson<Tweet>(response);
            Debug.Log("success! :" + response);

            //ツイート成功をコールバック関数に渡す
            delFuncs.callbackSuccess();

        } else {
            Error[] erros = JsonHelper.FromJson<Error>(response);

            if (erros != null) {
                if (erros[0].code == 89) {
                    //認証エラーならセッションをクリアし再ログイン
                    if (!isRetry) {
                        SessionRetry();
                    } else {
                        delFuncs.callbackFailed(response);
                        return;
                    }
                } else {
                    delFuncs.callbackFailed(response);
                    return;
                }
            }

        }
    }

    private void LocalTweetMakeWithMedia(TwitterSession session) {

        var auth = session.authToken;
        var accessToken = auth.token;
        var accessTokenSecret = auth.secret;

        Twity.Oauth.consumerKey = TwitterSettings.ConsumerKey;
        Twity.Oauth.consumerSecret = TwitterSettings.ConsumerSecret;
        Twity.Oauth.accessToken = accessToken;
        Twity.Oauth.accessTokenSecret = accessTokenSecret;

        byte[] imgBinary = File.ReadAllBytes(tweetMedia);
        string imgbase64 = Convert.ToBase64String(imgBinary);

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters["media_data"] = imgbase64;
        //parameters["additional_owners"] = "additional owner if you have";
        StartCoroutine(Twity.Client.Post("media/upload", parameters, MediaUploadCallback));

    }

    void MediaUploadCallback(bool success, string response) {
        if (success) {
            UploadMedia media = JsonUtility.FromJson<UploadMedia>(response);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["media_ids"] = media.media_id.ToString();
            parameters["status"] = tweetMassage;
            StartCoroutine(Twity.Client.Post("statuses/update", parameters, StatusesUpdateCallback));
        } else {
            Debug.Log(response);
            Error[] erros = JsonHelper.FromJson<Error>(response);

            if (erros != null) {
                if (erros[0].code == 89) {
                    //認証エラーならセッションをクリアし再ログイン
                    if (!isRetry) {
                        SessionRetry();
                    }
                } else {
                    delFuncs.callbackFailed(response);
                    return;
                }
            } else {
                delFuncs.callbackFailed(response);
                return;
            }

        }
    }

    void StatusesUpdateCallback(bool success, string response) {
        if (success) {
            Tweet tweet = JsonUtility.FromJson<Tweet>(response);
            Debug.Log("success_m! :" + response);

            //ツイート成功をコールバック関数に渡す
            delFuncs.callbackSuccess();

        } else {
            Error[] erros = JsonHelper.FromJson<Error>(response);

            if (erros != null) {
                if (erros[0].code == 89) {
                    //認証エラーならセッションをクリアし再ログイン
                    if (!isRetry) {
                        SessionRetry();
                    } else {
                        delFuncs.callbackFailed(response);
                        return;
                    }
                }
            } else {
                delFuncs.callbackFailed(response);
                return;
            }

        }
    }


    public static class JsonHelper {
        public static T[] FromJson<T>(string json) {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.errors;
        }

        [Serializable]
        private class Wrapper<T> {
            public T[] errors;

            private void dummy() {
                errors = null;
            }
        }
    }


}

