import React, { useEffect, useState } from 'react';

const Home: React.FC = () => {
    const [date, setDate] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetch('/api/date')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch date');
                }
                return response.json();
            })
            .then(data => setDate(data.date))
            .catch(err => setError('Error fetching date'));
    }, []);

    return (
        <div>
            <h1>Home Page</h1>
            {error ? (
                <p style={{ color: 'red' }}>{error}</p>
            ) : (
                <p>Date from API: {date ? date : 'Loading...'}</p>
            )}
        </div>
    );
};

export default Home;
