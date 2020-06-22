# Telegram Fangzi Bot

A private bot which can talk , speak and play music.


## config
Add a `appsettings.json` to the project root.

```json
{
    "DefaultReply": "喵喵喵？？",
    "TelegramAccessToken": "...",
    "SpeechSubscription": "...",
    "SpeechRegion": "...",
    "TulingApiKey": "...",
    "MasterUsers": []
}
```

## run

```shell
./run.sh
```

If you have problem with text2Speech service, please remove the SSL_CERT_DIR environment.
