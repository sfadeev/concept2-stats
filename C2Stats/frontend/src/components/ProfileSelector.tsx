import { AutoComplete, AutoCompleteProps } from "antd";
import { useEffect, useState } from "react";

export interface Profile {
    id: number;
    name: string;
}

export interface ProfileSelectorProps {
    profileId?: number;
    onChange: (value?: number) => void;
}

const getLocalStorageProfile = (): Profile => {
    return JSON.parse(localStorage.getItem('profile') || '{}');
};

const setLocalStorageProfile = (profile: Profile | null): void => {
    if (profile) localStorage.setItem('profile', JSON.stringify(profile));
    else localStorage.removeItem('profile');
};

export default () => {

    const [profile, setProfile] = useState<Profile | null>(getLocalStorageProfile);
    const [options, setOptions] = useState<AutoCompleteProps['options']>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => setLocalStorageProfile(profile), [profile]);

    const onSearch = (value: string) => {
        fetch(`/api/wod/profiles?search=${value}`)
            .then(response => {
                if (response.ok) return response.json();
                throw new Error(`${response.statusText} (${response.status})`);
            })
            .then((data: Profile[]) => {
                setOptions(() => {
                    return data.map((p) => ({ value: `${p.name}`, key: p.id }));
                });
            })
            .catch((e: Error) => {
                setOptions([]);
                return setError(e.message);
            });
    };

    return (<>
        <AutoComplete
            placeholder='Select Profile'
            defaultValue={profile?.name}
            allowClear
            options={options}
            style={{ width: 205 }}
            onSearch={onSearch}
            onSelect={(value: string, profile: any) => setProfile({ id: profile.key, name: profile.value })}
            onClear={() => setProfile(null)}
        />
    </>);

};
