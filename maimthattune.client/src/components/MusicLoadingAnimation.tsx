import React, { useState, useEffect } from 'react';

// Music symbols for the loading animation
const MUSIC_SYMBOLS = ['𝄞', '𝄞', '𝄢', '♯', '♭', '♮', '𝄽', '𝄽', '𝅗𝅥', '𝅘𝅥', '𝅘𝅥', '𝅘𝅥𝅮', '𝅘𝅥𝅯', '♫', '♫', '♬', '♬'];
const NUM_NOTES = 6;

interface MusicLoadingOverlayProps {
	fadingOut: boolean;
}

export interface NoteConfig {
	symbol: string;
	left: number; // percent
	bottom: number; // px
	fontSize: number; // rem
	animationDuration: number; // s
	animationDelay: number; // s
	fadeInDelay: number; // s - new property for fade-in timing
}

function getRandomInt(min: number, max: number): number {
	return Math.floor(Math.random() * (max - min + 1)) + min;
}

export function generateRandomNotes(): NoteConfig[] {
	return Array.from({ length: NUM_NOTES }, () => ({
		symbol: MUSIC_SYMBOLS[getRandomInt(0, MUSIC_SYMBOLS.length - 1)],
		left: getRandomInt(5, 95), // percent
		bottom: getRandomInt(10, 40), // px
		fontSize: getRandomInt(4, 8), // rem
		animationDuration: getRandomInt(25, 45) / 10.0, // 2.5s - 4.5s
		animationDelay: getRandomInt(0, 20) / 10.0, // 0s - 2.0s
		fadeInDelay: getRandomInt(0, 15) / 10.0, // 0s - 1.5s for staggered fade-in
	}));
}

/* Loads overlay component with animated music symbols that fade in and out */
export const MusicLoadingOverlay: React.FC<MusicLoadingOverlayProps> = ({ fadingOut }) => {
	const [notes, setNotes] = useState<NoteConfig[]>([]);

	useEffect(() => {
		// Generate random notes on mount
		setNotes(generateRandomNotes());
	}, []);

	return (
		<div className={`music-loading-overlay${fadingOut ? ' fade-out' : ''}`}>
			{notes.map((note, i) => (
				<span
					key={i}
					className="music-note music-note-fade-in"
					style={{
						left: `${note.left}%`,
						bottom: `${note.bottom}px`,
						fontSize: `${note.fontSize}rem`,
						animationDelay: `${note.fadeInDelay}s, ${note.animationDelay + note.fadeInDelay}s`,
						animationDuration: `0.8s, ${note.animationDuration}s`,
					}}
				>
					{note.symbol}
				</span>
			))}
		</div>
	);
};