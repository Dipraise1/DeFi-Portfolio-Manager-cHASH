import React, { useState, useEffect } from 'react';
import axios from 'axios';

const TokenBalances = ({ walletAddress }) => {
  const [tokenBalances, setTokenBalances] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTokenBalances = async () => {
      try {
        setLoading(true);
        const response = await axios.get(`/api/portfolio/${walletAddress}/tokens`);
        setTokenBalances(response.data);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch token balances:', err);
        setError('Failed to load token balance data');
      } finally {
        setLoading(false);
      }
    };

    if (walletAddress) {
      fetchTokenBalances();
    }
  }, [walletAddress]);

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  };

  const formatNumber = (value, decimals = 6) => {
    return new Intl.NumberFormat('en-US', {
      maximumFractionDigits: decimals
    }).format(value);
  };

  const formatPercentage = (value) => {
    return new Intl.NumberFormat('en-US', {
      style: 'percent',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value / 100);
  };

  if (loading) {
    return <div className="loading">Loading token balances...</div>;
  }

  if (error) {
    return <div className="error">{error}</div>;
  }

  return (
    <div>
      <h1>Token Balances</h1>
      
      <div className="card">
        <h3 className="card-title">Your Assets</h3>
        
        {tokenBalances.length === 0 ? (
          <p>No token balances found</p>
        ) : (
          <table className="table">
            <thead>
              <tr>
                <th>Token</th>
                <th>Balance</th>
                <th>Price (USD)</th>
                <th>Value (USD)</th>
                <th>24h Change</th>
                <th>Blockchain</th>
              </tr>
            </thead>
            <tbody>
              {tokenBalances.map((balance, index) => (
                <tr key={index}>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                      {balance.token.logoUrl && (
                        <img 
                          src={balance.token.logoUrl} 
                          alt={balance.token.symbol} 
                          className="token-icon" 
                        />
                      )}
                      <div>
                        <div>{balance.token.symbol}</div>
                        <div style={{ fontSize: '0.8rem', color: 'var(--text-light)' }}>
                          {balance.token.name}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td>{formatNumber(balance.balance)}</td>
                  <td>{formatCurrency(balance.token.currentPriceUsd)}</td>
                  <td>{formatCurrency(balance.balanceInUsd)}</td>
                  <td className={balance.token.priceChangePercentage24h >= 0 ? 'positive' : 'negative'}>
                    {formatPercentage(balance.token.priceChangePercentage24h)}
                  </td>
                  <td>{balance.token.blockchain}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default TokenBalances; 