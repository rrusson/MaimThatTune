import React, { useState } from 'react';
import './App.css';

interface GuessResult {
	isCorrect: boolean;
	artist: string;
	track: string;
}

function App() {
	const [audioUrl, setAudioUrl] = useState<string | null>(null);
	const [trackId, setTrackId] = useState<string | null>(null);
	const [guess, setGuess] = useState('');
	const [result, setResult] = useState<GuessResult | null>(null);
	const [loading, setLoading] = useState(false);
	const [gameStarted, setGameStarted] = useState(false);

	async function fetchRandomSegment() {
		setLoading(true);
		setResult(null);
		setGuess('');
		setAudioUrl(null);
		setTrackId(null);
		const response = await fetch('/api/music/random-segment');
		if (response.ok) {
			const blob = await response.blob();
			const id = response.headers.get('x-track-id');
			setAudioUrl(URL.createObjectURL(blob));
			setTrackId(id);
		}
		setLoading(false);
	}

	async function startGame() {
		setGameStarted(true);
		await fetchRandomSegment();
	}

	async function submitGuess(e: React.FormEvent) {
		e.preventDefault();
		if (!trackId) {
			return;
		}

		const response = await fetch('/api/music/guess', {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ trackId, guess })
		});
		if (response.ok) {
			const data = await response.json();
			setResult(data);
		}
	}

	if (!gameStarted) {
		return (
			<div className="start-modal-overlay">
				<div className="start-modal-content">
					<h1 className="start-modal-title">
						🎵 Maim That Tune! 🎵
					</h1>
					<p className="start-modal-description">
						Listen to music clips and guess the artist or track name.
					</p>
					<button
						onClick={startGame}
						className="start-button"
					>
						🎮 START 🎮
					</button>
				</div>
			</div>
		);
	}

	// Precompute isExactMatch for partial credit
	const isExactMatch = result && result.isCorrect &&
		(result.artist.toLowerCase() !== guess.toLowerCase() && result.track.toLowerCase() !== guess.toLowerCase());

	return (
		<div className="game-container">
			<h1 className="game-title">🎵 Maim That Tune! 🎵</h1>
			<p className="game-description">
				Listen to the music clip and try to guess the <strong>artist name</strong> or <strong>track name</strong>.
			</p>

			{audioUrl && (
				<div className="audio-container">
					<audio
						src={audioUrl}
						autoPlay
						className="audio-player"
						controls={false}
					/>
				</div>
			)}

			<form onSubmit={submitGuess} className="guess-form">
				<div className="guess-form-container">
					<input
						type="text"
						value={guess}
						onChange={e => setGuess(e.target.value)}
						placeholder="Enter artist or track name"
						disabled={!!result || loading}
						className="guess-input"
					/>
					<button
						type="submit"
						disabled={!guess || !!result || loading}
						className="guess-button"
					>
						Guess
					</button>
				</div>
			</form>

			{result && (
				<div className={`result-container ${isExactMatch ? 'result-partial-credit' : result.isCorrect ? 'result-correct' : 'result-wrong'}`}>
					<div className="result-message">
						{result.isCorrect ? (
							isExactMatch ? (
                                <span className="result-text-desaturated">🧲💣❤️ Close Enough ❤️💣🧲</span>
							) : (
								<span className="result-text">🎉 Correct! 🎉</span>
							)
						) : (
								<span className="result-text">😢 Wrong! 😢</span>
						)}
					</div>
					<div className="result-details">
						<div><strong>Artist:</strong> {result.artist}</div>
						<div><strong>Track:</strong> {result.track}</div>
					</div>
				</div>
			)}

			<div className="action-container">
				<button
					onClick={fetchRandomSegment}
					disabled={loading}
					className="next-track-button"
				>
					{loading ? 'Loading...' : '🔄 Guess Another Track'}
				</button>
			</div>
		</div>
	);
}

export default App;