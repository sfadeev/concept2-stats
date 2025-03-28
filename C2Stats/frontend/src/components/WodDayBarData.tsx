import { LinkOutlined } from '@ant-design/icons';
import { Alert, Col, Divider, Empty, Row, Space, Statistic, Typography } from 'antd';
import { useEffect, useState } from "react";
import { getDateString } from '../services/dateService';
import WodDayBar, { WodDayDataItem } from "./WodDayBar";

export interface Wod {
    id: number;
    type?: string;
    date?: Date;
    name?: string;
    url?: string;
    description?: string;
    totalCount?: number;
}

export interface WodDayData {
    wod?: Wod;
    data?: WodDayDataItem[];
}

export interface WodDayBarDataProps {
    type: string;
    date: Date;
    country?: string | null;
}

export default ({ type, date, country }: WodDayBarDataProps) => {

    const [loading, setLoading] = useState<boolean>(false);
    const [data, setData] = useState<WodDayData>({});
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {

        setLoading(true);

        fetch(`/api/wod/day?type=${type}&date=${getDateString(date)}&country=${country || ''}`)
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
                setData({});
                setLoading(false);
                return setError(e.message);
            });
    }, [type, date, country]);

    if (error) return <Alert message={error} type="error" />;

    const countryCount = country ? data?.data?.reduce((x, i) => x + (i.male || 0) + (i.female || 0), 0) : undefined;

    return (<>
        {data.wod ? (<Row gutter={16}>
            <Col span={20}>
                <Space direction="vertical" style={{ marginLeft: 40, marginRight: 40 }}>
                    <Typography.Title level={5} style={{ marginTop: 0 }}>{`${date?.toDateString()}`}</Typography.Title>
                    <Typography.Text strong>{data.wod.name}</Typography.Text>
                    <Typography.Link href={data.wod.url} target="_blank"><LinkOutlined /> {data.wod.url} </Typography.Link>
                    <Typography.Text type="secondary">{data.wod.description}</Typography.Text>
                </Space>
            </Col>
            <Col span={4}>
                <Space>
                    <Statistic loading={loading} title={`Total\u00A0People`} value={data.wod?.totalCount || ''} />
                    <Divider />
                    {country ? <Statistic loading={loading} title={country} value={countryCount} style={{ minWidth: 60 }} /> : null}
                </Space>
            </Col>
        </Row>) : null}

        {(data.data && data.data.length > 0) ? <WodDayBar data={data.data} /> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} />}
    </>);
};
