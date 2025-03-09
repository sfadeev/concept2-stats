import { AutoComplete, AutoCompleteProps } from "antd";
import React, { useEffect, useState } from "react";

export interface Profile {
    id: number;
    name: string;
}

export interface ProfileSelectorProps {
    profileId?: number;
    onChange: (value?: number) => void;
}

export default () => {

    const [profile, setProfile] = useState<any | null>(() => {
        const profile = JSON.parse(localStorage.getItem('profile') || '{}');
        return profile;
    });

    const [options, setOptions] = React.useState<AutoCompleteProps['options']>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (profile) localStorage.setItem('profile', JSON.stringify(profile));
        else localStorage.removeItem('profile');
    }, [profile]);

    const onSearch = (value: string) => {
        fetch(`/api/wod/profiles?search=${value}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch data');
                }
                return response.json();
            })
            .then((data: Profile[]) => {
                setOptions(() => {
                    return data.map((p) => ({ value: `${p.name}`, key: p.id }));
                });
            })
            .catch(err => setError('Error fetching data'));

    };

    return (<>
        <AutoComplete
            placeholder='Select Profile'
            defaultValue={profile?.value}
            allowClear={true}
            options={options}
            style={{ width: 205 }}
            onSearch={onSearch}
            onSelect={(value: string, profile: any) => setProfile(profile)}
            onClear={() => setProfile(null)}
        />
    </>);

};
