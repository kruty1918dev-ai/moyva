using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class SocialLinksServiceTests
    {
        private SocialLinksConfigSO _config;
        private List<string> _opened;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<SocialLinksConfigSO>();
            _opened = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void OpenLink_ByValidId_CallsOpenerWithUrl()
        {
            _config.SetEntriesForTest(new[]
            {
                new SocialLinkEntry { Id = "discord", Url = "https://discord.gg/x", DisplayName = "Discord" },
                new SocialLinkEntry { Id = "yt",      Url = "https://youtube.com/x", DisplayName = "YouTube" }
            });
            var sut = new SocialLinksService(_config, url => _opened.Add(url));

            bool ok = sut.OpenLink("discord");

            Assert.IsTrue(ok);
            Assert.AreEqual(1, _opened.Count);
            Assert.AreEqual("https://discord.gg/x", _opened[0]);
        }

        [Test]
        public void OpenLink_IgnoresInvalidUrl()
        {
            _config.SetEntriesForTest(new[]
            {
                new SocialLinkEntry { Id = "bad", Url = "", DisplayName = "Broken" }
            });
            var sut = new SocialLinksService(_config, url => _opened.Add(url));

            bool ok = sut.OpenLink("bad");

            Assert.IsFalse(ok);
            Assert.IsEmpty(_opened);
        }

        [Test]
        public void OpenLink_UnknownId_ReturnsFalse()
        {
            _config.SetEntriesForTest(new[]
            {
                new SocialLinkEntry { Id = "discord", Url = "https://discord.gg/x" }
            });
            var sut = new SocialLinksService(_config, url => _opened.Add(url));

            Assert.IsFalse(sut.OpenLink("nonexistent"));
        }

        [Test]
        public void Links_SkipEntriesWithoutId()
        {
            _config.SetEntriesForTest(new[]
            {
                new SocialLinkEntry { Id = null,      Url = "https://a.com" },
                new SocialLinkEntry { Id = "",        Url = "https://b.com" },
                new SocialLinkEntry { Id = "discord", Url = "https://discord.gg/x" }
            });
            var sut = new SocialLinksService(_config, _ => { });

            Assert.AreEqual(1, sut.Links.Count);
            Assert.AreEqual("discord", sut.Links[0].Id);
        }
    }
}
