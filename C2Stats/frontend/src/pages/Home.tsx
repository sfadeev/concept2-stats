import { useEffect, useState } from 'react';
import WodCalendarData from '../components/WodCalendarData';

export default () => {
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
            <WodCalendarData wodType='rowerg' year={2025} />
            <WodCalendarData wodType='rowerg' year={2024} />
            <WodCalendarData wodType='rowerg' year={2023} />
        </div>
    );
};
