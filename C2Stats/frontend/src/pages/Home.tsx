import WodCalendarData from '../components/WodCalendarData';

export default () => {

    return (
        <div>
            <h3>rowerg</h3>
            <WodCalendarData year={2025} wodType='rowerg' />
            <WodCalendarData year={2024} wodType='rowerg' />
            <WodCalendarData year={2023} wodType='rowerg' />

            <h3>bikeerg</h3>
            <WodCalendarData year={2025} wodType='bikeerg' />
            <WodCalendarData year={2024} wodType='bikeerg' />
            <WodCalendarData year={2023} wodType='bikeerg' />

            <h3>skierg</h3>
            <WodCalendarData year={2025} wodType='skierg' />
            <WodCalendarData year={2024} wodType='skierg' />
            <WodCalendarData year={2023} wodType='skierg' />

            {/* <WodDayBarData wodType='rowerg' /> */}
        </div>
    );
};
