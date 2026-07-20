import { useEffect, useState, useRef } from "react";
import type { ExchangeRateListDto, ExchangeRateUpdate } from "@/types";
import * as customerService from "@/services/customer";
import { useNotifications } from "@/context/NotificationContext";
import { formatDate } from "@/components/ui";

export function ExchangeRatesPage() {
  const [rates, setRates] = useState<ExchangeRateListDto[]>([]);
  const [loading, setLoading] = useState(false);
  const { latestRates } = useNotifications();
  const prevRatesRef = useRef<ExchangeRateUpdate[] | null>(null);

  useEffect(() => {
    setLoading(true);
    customerService.getExchangeRates().then((r) => {
      if (r.success && r.data) setRates(r.data);
      setLoading(false);
    });
  }, []);

  useEffect(() => {
    if (!latestRates) return;
    if (latestRates === prevRatesRef.current) return;
    prevRatesRef.current = latestRates;

    setRates((prev) =>
      prev.map((r) => {
        const update = latestRates.find((u) => u.currencyCode === r.currencyCode);
        return update
          ? { ...r, rate: update.rate, rateDate: update.rateDate, source: update.source }
          : r;
      })
    );
  }, [latestRates]);

  return (
    <>
      <h1>Exchange Rates</h1>
      <p className="page-subtitle">Current currency values against Turkish Lira, updated from the published market rates.</p>

      <section className="rates-list-section">
        <div className="customer-card-heading">
          <div><span className="customer-eyebrow">Published rates</span><h2>Currency board</h2></div>
          <span className="rates-count-badge">{rates.length} currencies</span>
        </div>
        {rates.length === 0 && !loading ? (
          <div className="rates-empty-state">No exchange rates are available right now.</div>
        ) : (
          <div className="rates-card-grid">
            {rates.map((rate) => (
              <article className="rate-detail-card" key={rate.rateId}>
                <div className="rate-card-top"><span className="rate-currency-badge">{rate.currencyCode}</span><span className="rate-source">{rate.source}</span></div>
                <span className="rate-label">1 {rate.currencyCode} equals</span>
                <strong>{rate.rate.toFixed(4)} <small>TRY</small></strong>
                <div className="rate-date">Updated {formatDate(rate.rateDate)}</div>
              </article>
            ))}
          </div>
        )}
      </section>
    </>
  );
}
