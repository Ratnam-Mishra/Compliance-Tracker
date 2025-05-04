import React, { useEffect, useState } from "react";
import ReactECharts from "echarts-for-react";

type TopKeyWordsType = {
  keyword: string;
  count: number;
};

const TopKeywords: React.FC = () => {
  const [topKeywords, setTopkeywords] = useState<TopKeyWordsType[]>();

  useEffect(() => {
    const fetchTopKeywords = async () => {
      try {
        const response = await fetch(
          "https://localhost:7205/api/Compliance/GetKeywords"
        );
        if (!response.ok) {
          throw new Error("Failed to fetch employees data");
        }

        const data: { violationExplanation: string }[] = await response.json();

        // Count occurrences
        const keywordMap: Record<string, number> = {};
        data.forEach(({ violationExplanation }) => {
          const key = violationExplanation.trim();
          keywordMap[key] = (keywordMap[key] || 0) + 1;
        });

        // Convert to chart data
        const chartData: TopKeyWordsType[] = Object.entries(keywordMap).map(
          ([keyword, count]) => ({
            keyword,
            count,
          })
        );

        setTopkeywords(chartData);
      } catch (err: any) {
        console.error(err);
      }
    };
    fetchTopKeywords();
  }, []);

  const chartOption = {
    tooltip: {
      trigger: "item",
    },
    legend: {
      top: "bottom",
      textStyle: {
        color: "#6B7280",
        fontWeight: "500",
      },
    },
    series: [
      {
        name: "Top Keywords",
        type: "pie",
        radius: ["40%", "70%"],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: "#fff",
          borderWidth: 2,
        },
        label: {
          show: false,
          position: "center",
        },
        emphasis: {
          label: {
            show: true,
            fontSize: "18",
            fontWeight: "bold",
            color: "#1F2937",
          },
        },
        labelLine: {
          show: false,
        },
        data: topKeywords?.map((k) => ({
          value: k.count,
          name: k.keyword,
        })),
        color: ["#FA4616", "#004F71", "#E38971", "#850000"],
      },
    ],
  };

  return (
    <div className="bg-white rounded-2xl shadow-md p-6 hover:shadow-lg transition-shadow">
      <h2 className="text-2xl font-semibold text-neutral-900 mb-4">
        Top Triggered Keywords
      </h2>

      <ReactECharts
        option={chartOption}
        style={{ height: "300px", width: "100%" }}
      />
    </div>
  );
};

export default TopKeywords;
