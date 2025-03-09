import { Select, Typography } from "antd";
import { useEffect, useState } from "react";
const { Text } = Typography;

export interface CountryDatum {
    code: string;
    name: string;
    value: number;
}

export interface CountrySelectorProps {
    type?: string | null;
    date?: Date | null;
    country?: string;
    onChange: (value?: string) => void;
}

export default ({ type, date, country, onChange }: CountrySelectorProps) => {

    const [loading, setLoading] = useState<boolean>(false);
    const [countries, setCountries] = useState<CountryDatum[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const year = date?.getFullYear();

        if (type && year) {

            setLoading(true);

            fetch(`/api/wod/countries?type=${type}&year=${year}`)
                .then(response => {
                    if (response.ok) return response.json();
                    throw new Error(`${response.statusText} (${response.status})`);
                })
                .then(data => {
                    setCountries(data);
                    setLoading(false);
                    return setError(null);
                })
                .catch((e: Error) => {

                    setCountries([]);
                    setLoading(false);
                    return setError(e.message);
                });
        }

    }, [type, date?.getFullYear()]);

    const totalCount = countries.reduce((x, c) => x + c.value, 0),
        totalCountText = totalCount > 0 ? `(${totalCount})` : '';

    return (
        <Select
            placeholder={`Select Country ${totalCountText}`}
            loading={loading}
            status={error ? 'error' : undefined}
            notFoundContent={error}
            value={country}
            allowClear={true}
            showSearch={true}
            filterOption={true}
            optionFilterProp='label'
            style={{ width: 205 }}
            options={countries?.map(x => { return { value: x.code, label: x.name, count: x.value }; })}
            optionRender={(option) =>
                <SelectOption label={option.data.label} count={option.data.count} />
            }
            onSelect={(_, country) => onChange(country.value)}
            onClear={() => onChange(undefined)}
        />
    );
};

interface SelectOptionProps {
    label: string;
    count: number;
}

function SelectOption({ label, count }: SelectOptionProps) {
    return (<>
        {label}
        <Text style={{ float: 'right' }} type="secondary">{count}</Text>
    </>);
};
