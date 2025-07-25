using System.Collections;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace CurseOfNaga.Utils
{
    public class FileDataHelper
    {
        private CancellationTokenSource _cts;
        private string _fileData = "";
        private Texture _fileTexture;

        public FileDataHelper() { _fileData = ""; _fileTexture = null; _cts = new CancellationTokenSource(); }
        ~FileDataHelper() { _cts.Cancel(); }

        /// <summary>
        /// Get the file data as string from the specified Path using IEnumerator.
        /// Works for both PC, Android.
        /// <para> Not tested for anything other than "streamingAssetsPath". </para>
        /// </summary>
        /// <param name="filePath"> Path to the file </param>
        /// <returns>File data as string</returns>
        public async Task<string> GetFileData_Async(string filePath)
        {
            bool gotData = false;
            int emergencyExit = 0;
            IEnumerator getFileDataEnumerator = GetFileDataEnumerator(filePath);

            while (!gotData)
            {
                if (_fileData != "")
                    gotData = true;
                else if (emergencyExit > 30 || _cts.Token.IsCancellationRequested)
                {
                    Debug.LogError($"Unable to load PowerUp. Reading from file took too long. Aborting!!");
                    return "";
                }

                emergencyExit++;
                getFileDataEnumerator.MoveNext();
                await Task.Delay(10);
            }

            return _fileData;
        }

        private IEnumerator GetFileDataEnumerator(string url)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(url);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Unable to load Data. Error : {webRequest.error}");
                yield break;
            }

            _fileData = webRequest.downloadHandler.text;
        }


        /// <summary>
        /// Get the texture from the file at specified Path using IEnumerator.
        /// Works for both PC, Android.
        /// <para> Not tested for anything other than "streamingAssetsPath". </para>
        /// </summary>
        /// <param name="filePath"> Path to the file </param>
        /// <returns></returns>
        public async Task<Texture> GetFileImage_Async(string filePath)
        {
            bool gotData = false;
            int emergencyExit = 0;
            IEnumerator getFileImageEnumerator = GetFileImageEnumerator(filePath);

            while (!gotData)
            {
                if (_fileTexture != null)
                    gotData = true;
                else if (emergencyExit > 30 || _cts.Token.IsCancellationRequested)
                {
                    Debug.LogError($"Unable to load PowerUp. Reading from file took too long. Aborting!!");
                    return null;
                }

                emergencyExit++;
                getFileImageEnumerator.MoveNext();
                await Task.Delay(10);
            }

            return _fileTexture;
        }

        //https://discussions.unity.com/t/loading-hundreds-of-ui-images-using-addressables/847399
        IEnumerator GetFileImageEnumerator(string mediaUrl)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(mediaUrl);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Unable to load Data. Error : {webRequest.error}");
                yield break;
            }
            _fileTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
        }
    }
}