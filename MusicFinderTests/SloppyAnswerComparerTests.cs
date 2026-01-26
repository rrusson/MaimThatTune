using System.Data;

using MusicFinder;

namespace MusicFinderTests
{
	[TestClass]
	public sealed class SloppyAnswerComparerTests
	{
		[TestMethod]
		public void AreCloseEnough_ExactMatch_ReturnsTrue()
		{
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("Queen", "Queen"));
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("The The", "The The"));   // Special case with double "The" (first instance is ignored)
		}

		[TestMethod]
		public void AreCloseEnough_CaseInsensitive_ReturnsTrue()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("Bohemian Rhapsody", "bohemian rhapsody");
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void AreCloseEnough_LeadingThe_ReturnsTrue()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("The Beatles", "Beetles");
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void AreCloseEnough_MinorTypo_ReturnsTrue()
		{
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("Nirvana", "Nirvanna"));
			Assert.IsTrue(SloppyAnswerComparer.AreCloseEnough("X", "Y"));
		}

		[TestMethod]
		public void AreCloseEnough_PunctuationAndWhitespace_ReturnsTrue()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("AC/DC!", "ac dc");
			Assert.IsTrue(result);
		}

		[TestMethod]
		[DataRow("The Beatles (feat. Nine Inch Nails)", "The Beetles")]
		[DataRow("The Beatles, Ozzy Osborne, and Doris Day", "The Beetles")]
		[DataRow("The Beatles; Glen Gould; Metallica", "Beatles")]
		[DataRow("Blur & Debby Gibson", "Blur")]
		[DataRow("Gwar - featuring Doris Day", "Gwar")]
		[DataRow("Gwar (feat. Doris Day)", "Gwar")]
		public void AreCloseEnough_MessyExtraArtist_ReturnsTrue(string answer, string guess)
		{
			var result = SloppyAnswerComparer.AreCloseEnough(answer, guess);
			Assert.IsTrue(result);
		}

		//			string[] funkyIndicators = [",", ";", " feat ", " feat.", " featuring ", "(feat", " ft.", "(ft", "&"];

		[TestMethod]
		[DataRow("Head Like a Hole feat. Doris Day", "Head Like a Hole")]
		[DataRow("Head Like a Hole, featuring Doris Day", "Head Like a Hole")]
		[DataRow("Head Like a Hole (featuring Doris Day)", "Head Like a Hole")]
		[DataRow("Head Like a Hole (ft. Doris Day)", "Head Like a Hole")]
		[DataRow("Lithium - ft Doris Day)", "Lithium")]
		public void AreCloseEnough_MessyTrackWithFeaturing_ReturnsTrue(string answer, string guess)
		{
			var result = SloppyAnswerComparer.AreCloseEnough(answer, guess);
			Assert.IsTrue(result);
		}

		[TestMethod]
		[DataRow(null, "test guess")]
		[DataRow("test artist", null)]
		[DataRow("", "test guess")]
		[DataRow("test artist", "")]
		[DataRow("test artist", "?")]    // Only punctuation (boils down to an empty string)
		[DataRow("X", "??????")]
		public void AreCloseEnough_NullOrEmpty_ReturnsFalse(string answer, string guess)
		{
			Assert.IsFalse(SloppyAnswerComparer.AreCloseEnough(answer, guess));
		}

		[TestMethod]
		public void AreCloseEnough_DifferentStrings_ReturnsFalse()
		{
			var result = SloppyAnswerComparer.AreCloseEnough("Radiohead", "Coldplay");
			Assert.IsFalse(result);
		}
	}
}
