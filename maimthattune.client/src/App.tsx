import { useEffect, useState } from 'react';
import './App.css';

interface GuessResult {
    isCorrect: boolean;
    artist: string;
}

function App() {
    const [audioUrl, setAudioUrl] = useState<string | null>(null);
    const [trackId, setTrackId] = useState<string | null>(null);
    const [guess, setGuess] = useState('');
    const [result, setResult] = useState<GuessResult | null>(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        fetchRandomSegment();
    }, []);

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

    async function submitGuess(e: React.FormEvent) {
        e.preventDefault();
        if (!trackId) return;
        
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

    return (
        <div>
            <h1>Guess the Artist</h1>
            <p>Listen to the 5-second music clip and guess the artist!</p>
            {audioUrl && (
                <audio controls src={audioUrl} autoPlay />
            )}
            <form onSubmit={submitGuess} style={{ marginTop: 16 }}>
                <input
                    type="text"
                    value={guess}
                    onChange={e => setGuess(e.target.value)}
                    placeholder="Enter artist name"
                    disabled={!!result || loading}
                />
                <button type="submit" disabled={!guess || !!result || loading}>Guess</button>
            </form>
            {result && (
                <div style={{ marginTop: 16 }}>
                    {result.isCorrect ? (
                        <span style={{ color: 'green' }}>Correct! 🎉</span>
                    ) : (
                        <span style={{ color: 'red' }}>Wrong! 😢</span>
                    )}
                    <div>Artist: <b>{result.artist}</b></div>
                </div>
            )}
            <button onClick={fetchRandomSegment} disabled={loading} style={{ marginTop: 24 }}>
                Guess Another Track
            </button>
        </div>
    );
}

export default App;