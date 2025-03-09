import { Select, Typography } from "antd";
import { useEffect, useState } from "react";
const { Text } = Typography;

export interface Country {
    code: string;
    name: string;
}

export interface CountryDatum {
    code: string;
    name: string;
    value: number;
}

export interface CountrySelectorProps {
    type?: string | null;
    date?: Date | null;
    onChange: (value?: string) => void;
}

const getLocalStorageCountry = (): Country => {
    return JSON.parse(localStorage.getItem('country') || '{}');
};

const setLocalStorageCountry = (country: Country | null): void => {
    if (country) localStorage.setItem('country', JSON.stringify(country));
    else localStorage.removeItem('country');
};

export default ({ type, date, onChange }: CountrySelectorProps) => {

    const [loading, setLoading] = useState<boolean>(false);
    const [country, setCountry] = useState<Country | null>(getLocalStorageCountry);
    const [countries, setCountries] = useState<CountryDatum[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        onChange(country?.code);
        return setLocalStorageCountry(country);
    }, [country]);

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

    const onSelect = (country: Country | null) => {
        setCountry(country);
        onChange(country?.code);
    };

    const totalCount = countries.reduce((x, c) => x + c.value, 0),
        totalCountText = totalCount > 0 ? `(${totalCount})` : '';

    return (
        <Select
            placeholder={`Select Country ${totalCountText}`}
            loading={loading}
            status={error ? 'error' : undefined}
            notFoundContent={error}
            defaultValue={country?.code}
            allowClear={true}
            showSearch={true}
            filterOption={true}
            optionFilterProp='label'
            style={{ width: 205 }}
            options={countries?.map(x => { return { value: x.code, label: x.name, count: x.value }; })}
            optionRender={(option) =>
                <SelectOption label={option.data.label} count={option.data.count} />
            }
            onSelect={(_, country) => onSelect({ code: country.value, name: country.label })}
            onClear={() => onSelect(null)}
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
