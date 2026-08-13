using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TooManyEmotes;
using TooManyEmotes.Audio;
using UnityEngine;

namespace TooManyEmotesAdditionalEmotes
{
    internal class EmoteLoader
    {
        public static void LoadAssets()
        {
            string assetPath = Path.Combine(Path.GetDirectoryName(Plugin.instance.Info.Location), "dances");

            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetPath);
            LoadAnimationAssets(assetBundle);
            LoadAudioAssets(assetBundle);
            EmotesManager.BuildEmotesList();

            AdditionalEmoteData.SetAdditionalEmoteData();
            AdditionalEmoteData.SetAdditionalPropData();
            AdditionalEmoteData.SetAdditionalMusicData();
            
            Plugin.Logger.LogInfo("Loaded emotes!");
        }

        static void LoadAnimationAssets(AssetBundle assetBundle)
        {
            try
            {
                var animationClips = assetBundle.LoadAllAssets<AnimationClip>();
                TooManyEmotes.Plugin.complementaryAnimationClips.AddRange(animationClips);
                TooManyEmotes.Plugin.customAnimationClipsHash.UnionWith(animationClips);
                TooManyEmotes.Plugin.customAnimationClips.AddRange(animationClips);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Unable to load animations from {assetBundle}");
            }
        }

        // In order to support DMCA free audio assets, I would need to care about re-loading audio when DMCA toggle is pressed
        static void LoadAudioAssets(AssetBundle assetBundle)
        {
            try
            {
                var audioClips = assetBundle.LoadAllAssets<AudioClip>();
                Dictionary<string, AudioClip> audioDict = (Dictionary<string, AudioClip>)Traverse.CreateWithType("AudioManager").Field("audioClipsDictDmca").GetValue();
                HashSet<AudioClip> loadedAudioClips = (HashSet<AudioClip>)Traverse.CreateWithType("AudioManager").Field("loadedAudioClips").GetValue();
                HashSet<AudioClip> loadedAudioClipsDmca = (HashSet<AudioClip>)Traverse.CreateWithType("AudioManager").Field("loadedAudioClipsDmca").GetValue();
                if (audioDict != null && loadedAudioClips != null && loadedAudioClipsDmca != null)
                {
                    foreach (var clip in audioClips)
                    {
                        if (!audioDict.ContainsKey(clip.name))
                        {
                            audioDict[clip.name] = clip;
                        }
                        if (!loadedAudioClips.Contains(clip)) { loadedAudioClips.Add(clip); }
                        if (!loadedAudioClipsDmca.Contains(clip)) { loadedAudioClipsDmca.Add(clip); }
                    }
                    Plugin.Logger.LogInfo("Loaded audio clips for emotes.");
                }
                else
                {
                    Plugin.Logger.LogError("Failed to retrieve Audio Dictionary or Audio Set");
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Unable to load audio from {assetBundle}");
            }
        }
    }
}
