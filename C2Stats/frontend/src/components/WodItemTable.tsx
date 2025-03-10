import { Alert, Table } from "antd";
import { ColumnType } from "antd/es/table";
import { useEffect, useState } from "react";
import { getDateString } from "../services/dateService";

export interface WodItemTableProps {
    type: string;
    date: Date;
    country?: string | null;
}

export interface WodItem {
    no: number;
    profileId: number;
    position: number;
    name: string;
    sex: string;
    age: number;
    location: string;
    country: string;
    resultTime: any;
    resultTimeFmt: string;
    resultMeters: number;
    pace: any;
    paceFmt: string;
}

const WodItemTable = ({ type, date, country }: WodItemTableProps) => {

    const [loading, setLoading] = useState<boolean>(false);
    const [data, setData] = useState<WodItem[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {

        setLoading(true);

        fetch(`/api/wod/wodItems?type=${type}&date=${getDateString(date)}&country=${country || ''}`)
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
                setData([]);
                setLoading(false);
                return setError(e.message);
            });
    }, [type, date, country]);

    if (error) return <Alert message={error} type="error" />;

    // if (loading) return <Skeleton paragraph={{ rows: 8 }} style={{ height: 220 }} />;

    const resultTimeHidden = data.every(x => x.resultTime == null);
    const resultMetersHidden = data.every(x => x.resultMeters == null);

    const columns: ColumnType[] = [
        {
            title: 'No.',
            dataIndex: 'no'
        },
        {
            title: 'Pos.',
            dataIndex: 'position'
        },
        {
            title: 'Name',
            dataIndex: 'name',
        },
        {
            title: 'Age',
            dataIndex: 'age',
        },
        {
            title: 'Location',
            dataIndex: 'location',
        },
        {
            title: 'Country',
            dataIndex: 'country',
        },
        {
            title: 'Result',
            dataIndex: 'resultTimeFmt',
            hidden: resultTimeHidden
        },
        {
            title: 'Result',
            dataIndex: 'resultMeters',
            hidden: resultMetersHidden
        },
        {
            title: 'Pace',
            dataIndex: 'paceFmt',
        }
    ];

    return (<>
        <Table
            loading={loading}
            rowKey={'profileId'}
            size={'small'}
            // pagination={{ pageSize: 50 }}
            dataSource={data}
            columns={columns}
        />
    </>);
};

export default WodItemTable;
