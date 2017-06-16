using System;
using System.Collections.Generic;
using System.IO;
using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Properties;
using NAudio.Wave;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public static class Audios
    {
        private static Random rnd = new Random();

        public static bool EnabledAudio;

        public static void EnableSmooth()
        {
            PlayWav("Orc - I would not do such things if I were you.wav");
        }

        public static void DisableAudio()
        {
            PlayWav("DeathKnight - When my work is finished, I'm coming back for you.wav");
        }

        public static IWavePlayer PlayBackgroundMusic()
        {
            return PlayMp3("13_Orc_-_Rushed-ihQ.mp3", true);
        }

        public static void Tree()
        {
            PlayWav(@"Misc\Tree" + rnd.Next(1, 4) + ".wav");
        }

        public static void BaseHit()
        {
            PlayWav(@"Misc\Explode.wav");
        }

        public static void TowerHit()
        {
            PlayWav(@"Misc\Explode.wav");
        }

        public static void KillWizard_Union()
        {
            PlayWav(@"Misc\Raise Dead.wav");
        }

        public static void KillWizard_Enemy()
        {
            PlayWav(@"Human\Hdead.wav");
        }

        public static void KillMinion_Union()
        {
            PlayWav(@"Orc\Odead.wav");
        }

        public static void KillMinion_Enemy()
        {
            PlayWav(@"Human\Hdead.wav");
        }

        public static void BaseAttacked_Union()
        {
            PlayWav(@"Orc\Ohelp2.wav");
        }

        public static void TowerAttacked_Union()
        {
            PlayWav(@"Orc\Ohelp1.wav");
        }

        public static void DestroyBuilding()
        {
            PlayWav(@"Misc\Bldexpl" + rnd.Next(1, 3) + ".wav");
        }

        public static void SwordHit()
        {
            PlayWav(@"Misc\Sword" + rnd.Next(1, 3) + ".wav");
        }

        public static void Axe()
        {
            PlayWav(@"Misc\Axe.wav");
        }

        public static void OrcHit()
        {
            PlayWav(@"Misc\Fist.wav");
        }

        public static void Arrow()
        {
            PlayWav(@"Misc\Bowfire.wav");
        }

        public static void Fireball()
        {
            PlayWav(@"Misc\Burning.wav");
        }

        public static void FrostBolt()
        {
            PlayWav(@"Spells\Icestorm.wav");
        }

        public static void Haste()
        {
            PlayWav(@"Spells\Haste.wav");
        }

        public static void Shield()
        {
            PlayWav(@"Spells\Heal.wav");
        }

        public static void MagicMissile_Union()
        {
            PlayWav(@"Spells\Touchdrk.wav");
        }

        public static void MagicMissile_Enemy()
        {
            PlayWav(@"Spells\Spell.wav");
        }

        public static void Union_Base_Selected()
        {
            PlayWav("blacksmith.wav");
        }

        public static void Enemy_Base_Selected()
        {
            PlayWav("blacksmith.wav");
        }

        public static void Union_Tower_Selected()
        {
            PlayWav(@"Troll\Trpissd3.wav");
        }

        public static void Enemy_Tower_Selected()
        {
            PlayWav(@"Human\Hpissed3.wav");
        }

        public static void Wizard_Union_Selected()
        {
            PlayWav("DeathKnight - Yes.wav");
        }

        public static void Wizard_Union_Start()
        {
            PlayWav("DeathKnight - Make it quick.wav");
        }

        public static void Wizard_Selected_Union()
        {
            PlayWav($@"DeathKnt\Dkwhat{new Random().Next(1, 2)}.wav");
        }

        public static void Wizard_Selected_Enemy()
        {
            PlayWav($@"Wizard\Wzpissd{new Random().Next(1, 3)}.wav");
        }

        public static void Minion_Selected_Union(bool isAxe)
        {
            if (isAxe)
            {
                PlayWav($@"Troll\Trwhat{new Random().Next(1, 3)}.wav");
            }
            else
            {
                PlayWav($@"Orc\Owhat{new Random().Next(1, 6)}.wav");
            }
        }

        public static void Minion_Selected_Enemy(bool isArcher)
        {
            if (isArcher)
            {
                PlayWav($@"Elves\Epissed{new Random().Next(1, 3)}.wav");
            }
            else
            {
                PlayWav($@"Human\Hpissed{new Random().Next(1, 6)}.wav");
            }
        }

        private static void PlayWav(string fileName)
        {
            if (!EnabledAudio) return;
            IWavePlayer player = new WaveOutEvent();
            var ms = new MemoryStream(GetFile(fileName));
            var fr = new WaveFileReader(ms);
            player.Init(fr);
            player.Play();
            player.PlaybackStopped += (sender, args) => player.Dispose();
        }

        private static IWavePlayer PlayMp3(string fileName, bool loop)
        {
            if (!EnabledAudio) return null;
            IWavePlayer player = new DirectSoundOut();
            var ms = new MemoryStream(GetFile(fileName));
            var fr = new Mp3FileReader(ms);
            if (loop)
            {
                LoopStream ls = new LoopStream(fr);
                player.Init(ls);
            }
            else
            {
                player.Init(fr);
            }
            player.Play();
            return player;
        }

        private static byte[] GetFile(string file)
        {
            byte[] bytes;
            if (!Cache.TryGetValue(file, out bytes))
            {
                var path = Path.Combine(Settings.Default.ResourcesDir, "WarCraft 2 Audio", file);
                if (!File.Exists(path))
                {
                    path = Path.Combine(Settings.Default.ResourcesDir, "WarCraft 2 Sounds", file);
                }
                bytes = File.ReadAllBytes(path);
                Cache.Add(file, bytes);
            }
            return bytes;
        }

        private static Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();
    }
}