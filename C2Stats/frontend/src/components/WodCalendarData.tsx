import { Alert } from "antd";
import { useEffect, useState } from "react";
import WodCalendar, { WodCalendarProps } from "./WodCalendar";

export interface WodCalendarDataProps {
    type: string;
    year: number;
    country?: string | null;
    profileId?: number | null;
    scope?: string | null;
    onClick?: ((date: Date, event: React.MouseEvent<SVGRectElement, MouseEvent>) => void) | undefined;
}

export default ({ type, year, country, profileId, scope, onClick }: WodCalendarDataProps) => {

    const [loading, setLoading] = useState<boolean>(false);
    const [data, setData] = useState<WodCalendarProps | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {

        setLoading(true);

        const url = new URL("/api/wod/year", window.location.origin);

        url.searchParams.append('type', type);
        url.searchParams.append('year', year.toString());

        if (country) url.searchParams.append('country', country);
        if (profileId) url.searchParams.append('profileId', profileId.toString());
        if (scope) url.searchParams.append('scope', scope);

        fetch(url)
            .then(response => {
                if (response.ok) return response.json();
                throw new Error(`${response.statusText} (${response.status})`);
            })
            .then(data => {
                setData(data);
                setLoading(false);
                return setError(null);
            })
            .catch(e => {
                setData(null);
                setLoading(false);
                return setError(e.message);
            });
    }, [type, year, country, profileId, scope]);

    if (error) return <Alert message={error} type="error" />;

    return (<>
        {data ? (<WodCalendar
            from={data.from}
            to={data.to}
            data={data.data}
            onClick={(d, x) => { return (onClick ? onClick(d, x) : null); }}
        />) : null}
    </>);
};
