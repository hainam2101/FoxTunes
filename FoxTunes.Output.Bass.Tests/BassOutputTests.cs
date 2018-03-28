﻿using NUnit.Framework;
using System.Threading.Tasks;

namespace FoxTunes.Output.Bass.Tests
{
    [TestFixture(BassOutputTests.DEFAULT)]
    [TestFixture(BassOutputTests.DEFAULT | BassOutputTests.RESAMPLER)]
    [TestFixture(BassOutputTests.DEFAULT | BassOutputTests.ASIO)]
    [TestFixture(BassOutputTests.DEFAULT | BassOutputTests.RESAMPLER | BassOutputTests.ASIO)]
    public class BassOutputTests : TestBase
    {
        public const long RESAMPLER = 128;

        public const long ASIO = 256;

        public static int DS_DEVICE = -1;

        public static int ASIO_DEVICE = 0;

        public BassOutputTests(long configuration)
            : base(configuration)
        {

        }

        public override void SetUp()
        {
            base.SetUp();
            var output = this.Core.Components.Output as BassOutput;
            if (output == null)
            {
                Assert.Ignore("Requires \"{0}\".", typeof(BassOutput).FullName);
            }
            if ((this.Configuration & RESAMPLER) != 0)
            {
                output.Resampler = true;
            }
            else
            {
                output.Resampler = false;
            }
            if ((this.Configuration & ASIO) != 0)
            {
                output.Mode = BassOutputMode.ASIO;
                output.AsioDevice = ASIO_DEVICE;
            }
            else
            {
                output.Mode = BassOutputMode.DirectSound;
                output.DirectSoundDevice = DS_DEVICE;
            }
        }

        [Test]
        public async Task CanPlayStream()
        {
            var outputStream = await this.Core.Components.Output.Load(TestInfo.PlaylistItems[0]);
            outputStream.Play();
            for (var a = 0; a <= 15; a++)
            {
                await Task.Delay(1000);
                if (outputStream.Position == outputStream.Length)
                {
                    break;
                }
                else if (a == 15)
                {
                    Assert.Fail("Playback did not complete.");
                }
            }
        }

        [Test]
        public async Task CanPauseAndResumeStream()
        {
            var outputStream = await this.Core.Components.Output.Load(TestInfo.PlaylistItems[0]);
            outputStream.Play();
            await Task.Delay(1000);
            Assert.IsTrue(outputStream.Position > 0);
            outputStream.Pause();
            var position = outputStream.Position;
            await Task.Delay(1000);
            Assert.AreEqual(position, outputStream.Position);
            outputStream.Resume();
            await Task.Delay(1000);
            Assert.IsTrue(outputStream.Position > position);
        }

        [Test]
        public async Task CanSeekStream()
        {
            var outputStream = await this.Core.Components.Output.Load(TestInfo.PlaylistItems[0]);
            var quarter = outputStream.Length / 4;
            var half = outputStream.Length / 2;
            outputStream.Position = quarter;
            outputStream.Play();
            await Task.Delay(1000);
            Assert.IsTrue(outputStream.Position > quarter);
            outputStream.Position = half;
            await Task.Delay(1000);
            Assert.IsTrue(outputStream.Position > half);
        }

        [Test]
        public async Task CanPerformGaplessTransition()
        {
            var outputStreams = new[]
            {
                await this.Core.Components.Output.Load(TestInfo.PlaylistItems[2]),
                await this.Core.Components.Output.Load(TestInfo.PlaylistItems[3])
            };
            outputStreams[0].Play();
            await this.Core.Components.Output.Preempt(outputStreams[1]);
            outputStreams[0].Position = outputStreams[0].Length - 1000;
            await Task.Delay(1000);
            Assert.AreEqual(outputStreams[0].Length, outputStreams[0].Position);
            Assert.IsTrue(outputStreams[1].Position > 0);
        }
    }
}