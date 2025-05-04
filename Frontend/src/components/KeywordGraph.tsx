import React, { useEffect, useState } from "react";
import ReactECharts from "echarts-for-react";

type KeywordGraphPoint = {
  date: string; // e.g., "2025-04-20"
  count: number;
};

const KeywordGraph: React.FC = () => {
  const [graphData, setGraphData] = useState<KeywordGraphPoint[]>([]);

  useEffect(() => {
    const fetchTopKeywords = async () => {
      try {
        const response = await fetch("https://localhost:7205/api/Compliance/GetKeywords");
        if (!response.ok) {
          throw new Error("Failed to fetch compliance data");
        }

        const data: { time: string; violationExplanation: string }[] = await response.json();

        // Count violations per date
        const dateMap: Record<string, number> = {};
        data.forEach(({ time }) => {
          const dateOnly = new Date(time).toISOString().split("T")[0]; // "2025-04-20"
          dateMap[dateOnly] = (dateMap[dateOnly] || 0) + 1;
        });

        const chartData: KeywordGraphPoint[] = Object.entries(dateMap)
          .sort(([a], [b]) => new Date(a).getTime() - new Date(b).getTime()) // sort by date
          .map(([date, count]) => ({ date, count }));

        setGraphData(chartData);
      } catch (err: any) {
        console.error(err);
      }
    };

    fetchTopKeywords();
  }, []);

  const option = {
    tooltip: {
      trigger: "axis",
    },
    grid: {
      left: "3%",
      right: "4%",
      bottom: "3%",
      containLabel: true,
    },
    xAxis: {
      type: "category",
      boundaryGap: false,
      data: graphData.map((point) => point.date),
      axisLabel: { color: "#6B7280" },
    },
    yAxis: {
      type: "value",
      axisLabel: { color: "#6B7280" },
    },
    series: [
      {
        name: "Frequency",
        type: "line",
        smooth: true,
        lineStyle: {
          color: "#FA4616",
          width: 3,
        },
        itemStyle: {
          color: "#FA4616",
        },
        areaStyle: {
          color: "#FFECE6",
        },
        data: graphData.map((point) => point.count),
      },
    ],
  };

  return (
    <div className="bg-white rounded-2xl shadow-md p-6 hover:shadow-lg transition-shadow">
      <h2 className="text-2xl font-semibold text-neutral-900 mb-4">
        Keyword Frequency Over Time
      </h2>
      <ReactECharts
        option={option}
        style={{ height: "300px", width: "100%" }}
      />
    </div>
  );
};

export default KeywordGraph;
