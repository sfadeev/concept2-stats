import { CalendarDatum, ResponsiveCalendar } from '@nivo/calendar';

export interface WodCalendarProps {
    from: string;
    to: string;
    data: CalendarDatum[];
}

// https://nivo.rocks/calendar/
export default ({ from, to, data }: WodCalendarProps) => (

    <div style={{ width: "100%", height: 200 }}>

        <ResponsiveCalendar
            data={data ?? []}
            from={from}
            to={to}
            emptyColor="#eeeeee"
            colors={['#61cdbb', '#97e3d5', '#e8c1a0', '#f47560']}
            margin={{ top: 40, right: 40, bottom: 40, left: 40 }}
            yearSpacing={30}
            yearLegendOffset={20}
            monthBorderColor="#ffffff"
            dayBorderWidth={2}
            dayBorderColor="#ffffff"
            legends={[
                {
                    anchor: 'bottom-right',
                    direction: 'row',
                    translateY: 36,
                    itemCount: 4,
                    itemWidth: 42,
                    itemHeight: 36,
                    itemsSpacing: 14,
                    itemDirection: 'right-to-left'
                }
            ]}
        />

    </div>
);
