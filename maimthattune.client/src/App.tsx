import { useEffect, useState } from 'react';
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

    if (!gameStarted) {
        return (
            <div style={{
                position: 'fixed',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%',
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                zIndex: 1000
            }}>
                <div style={{
                    backgroundColor: 'white',
                    padding: '3rem',
                    borderRadius: '20px',
                    textAlign: 'center',
                    boxShadow: '0 10px 30px rgba(0, 0, 0, 0.3)'
                }}>
                    <h1 style={{ color: '#333', marginBottom: '2rem', fontSize: '2.5rem' }}>
                        🎵 Maim That Tune! 🎵
                    </h1>
                    <p style={{ color: '#666', marginBottom: '2rem', fontSize: '1.2rem' }}>
                        Listen to music clips and guess the artist or track name!
                    </p>
                    <button
                        onClick={startGame}
                        style={{
                            fontSize: '2rem',
                            padding: '1rem 3rem',
                            backgroundColor: '#ff6b6b',
                            color: 'white',
                            border: 'none',
                            borderRadius: '15px',
                            cursor: 'pointer',
                            fontWeight: 'bold',
                            textTransform: 'uppercase',
                            letterSpacing: '2px',
                            boxShadow: '0 5px 15px rgba(255, 107, 107, 0.4)',
                            transition: 'all 0.3s ease'
                        }}
                        onMouseOver={(e) => {
                            e.currentTarget.style.transform = 'scale(1.05)';
                            e.currentTarget.style.backgroundColor = '#ff5252';
                        }}
                        onMouseOut={(e) => {
                            e.currentTarget.style.transform = 'scale(1)';
                            e.currentTarget.style.backgroundColor = '#ff6b6b';
                        }}
                    >
                        🎮 START 🎮
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div style={{ padding: '2rem', maxWidth: '600px', margin: '0 auto' }}>
            <h1 style={{ textAlign: 'center', color: '#333' }}>🎵 Maim That Tune! 🎵</h1>
            <p style={{ textAlign: 'center', color: '#666', marginBottom: '2rem' }}>
                Listen to the music clip and try to guess the <strong>artist name</strong> or <strong>track name</strong>!
            </p>
            
            {audioUrl && (
                <div style={{ textAlign: 'center', marginBottom: '1.5rem' }}>
                    <audio controls src={audioUrl} autoPlay style={{ width: '100%', maxWidth: '400px' }} />
                </div>
            )}
            
            <form onSubmit={submitGuess} style={{ marginBottom: '1.5rem' }}>
                <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'center' }}>
                    <input
                        type="text"
                        value={guess}
                        onChange={e => setGuess(e.target.value)}
                        placeholder="Enter artist or track name"
                        disabled={!!result || loading}
                        style={{
                            padding: '0.75rem',
                            fontSize: '1rem',
                            border: '2px solid #ddd',
                            borderRadius: '8px',
                            flex: 1,
                            maxWidth: '300px'
                        }}
                    />
                    <button 
                        type="submit" 
                        disabled={!guess || !!result || loading}
                        style={{
                            padding: '0.75rem 1.5rem',
                            fontSize: '1rem',
                            backgroundColor: '#4CAF50',
                            color: 'white',
                            border: 'none',
                            borderRadius: '8px',
                            cursor: 'pointer',
                            fontWeight: 'bold'
                        }}
                    >
                        Guess
                    </button>
                </div>
            </form>

            {result && (
                <div style={{ 
                    textAlign: 'center',
                    padding: '1.5rem',
                    backgroundColor: result.isCorrect ? '#d4edda' : '#f8d7da',
                    borderRadius: '8px',
                    marginBottom: '1.5rem'
                }}>
                    <div style={{ fontSize: '1.5rem', marginBottom: '1rem' }}>
                        {result.isCorrect ? (
                            <span style={{ color: '#155724' }}>🎉 Correct! 🎉</span>
                        ) : (
                            <span style={{ color: '#721c24' }}>😢 Wrong! 😢</span>
                        )}
                    </div>
                    <div style={{ fontSize: '1.1rem' }}>
                        <div><strong>Artist:</strong> {result.artist}</div>
                        <div><strong>Track:</strong> {result.track}</div>
                    </div>
                </div>
            )}

            <div style={{ textAlign: 'center' }}>
                <button 
                    onClick={fetchRandomSegment} 
                    disabled={loading}
                    style={{
                        padding: '1rem 2rem',
                        fontSize: '1.1rem',
                        backgroundColor: '#2196F3',
                        color: 'white',
                        border: 'none',
                        borderRadius: '8px',
                        cursor: 'pointer',
                        fontWeight: 'bold'
                    }}
                >
                    {loading ? 'Loading...' : '🔄 Guess Another Track'}
                </button>
            </div>
        </div>
    );
}

export default App;