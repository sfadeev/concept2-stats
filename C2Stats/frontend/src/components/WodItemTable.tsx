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
    pid: number;
    pos: number;
    name: string;
    sex: string;
    age: number;
    loc: string;
    ctry: string;
    resT: any;
    resTF: string;
    resM: number;
    pace: any;
    paceF: string;
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

    data.forEach((x, index) => x.no = index + 1);
    const resultTimeHidden = data.every(x => x.resTF == null);
    const resultMetersHidden = data.every(x => x.resM == null);

    const columns: ColumnType[] = [
        {
            title: 'No.',
            dataIndex: 'no'
        },
        {
            title: 'Pos.',
            dataIndex: 'pos'
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
            dataIndex: 'loc',
        },
        {
            title: 'Country',
            dataIndex: 'ctry',
        },
        {
            title: 'Result, time',
            dataIndex: 'resTF',
            hidden: resultTimeHidden
        },
        {
            title: 'Result, m',
            dataIndex: 'resM',
            hidden: resultMetersHidden
        },
        {
            title: 'Pace',
            dataIndex: 'paceF',
        }
    ];

    return (<>
        <Table
            loading={loading}
            rowKey={'no'}
            size={'small'}
            dataSource={data}
            columns={columns}
        />
    </>);
};

export default WodItemTable;
