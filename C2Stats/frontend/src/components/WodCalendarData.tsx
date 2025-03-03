import { useEffect, useState } from "react";
import WodCalendar, { WodCalendarProps } from "./WodCalendar";

export interface WodCalendarDataProps {
    type: string;
    year: number;
    country?: string | null;
    onClick?: ((date: Date, event: React.MouseEvent<SVGRectElement, MouseEvent>) => void) | undefined;
}

export default ({ type, year, country, onClick }: WodCalendarDataProps) => {

    const [data, setData] = useState<WodCalendarProps | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        fetch(`/api/wod/year?type=${type}&year=${year}&country=${country || ''}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch data');
                }
                return response.json();
            })
            .then(data => {
                setData(data);
            })
            .catch(err => setError('Error fetching data'));
    }, [type, year, country]);

    return (
        <>
            {error ? (
                <p style={{ color: 'red' }}>{error}</p>
            ) : (
                data ? (
                    <WodCalendar
                        from={data.from}
                        to={data.to}
                        data={data.data}
                        onClick={(d, x) => { return (onClick ? onClick(d, x) : null); }}
                    />
                ) : (
                    <div>Loading...</div>
                )
            )}
        </>
    );
};
