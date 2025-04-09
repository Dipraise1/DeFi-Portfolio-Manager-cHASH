import React, { useState, useEffect } from 'react';
import axios from 'axios';

const YieldPositions = ({ walletAddress }) => {
  const [yieldPositions, setYieldPositions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchYieldPositions = async () => {
      try {
        setLoading(true);
        const response = await axios.get(`/api/portfolio/${walletAddress}/yield`);
        setYieldPositions(response.data);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch yield positions:', err);
        setError('Failed to load yield position data');
      } finally {
        setLoading(false);
      }
    };

    if (walletAddress) {
      fetchYieldPositions();
    }
  }, [walletAddress]);

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  };

  const formatPercentage = (value) => {
    return new Intl.NumberFormat('en-US', {
      style: 'percent',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value / 100);
  };

  const formatDate = (dateString) => {
    const options = { year: 'numeric', month: 'short', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  if (loading) {
    return <div className="loading">Loading yield positions...</div>;
  }

  if (error) {
    return <div className="error">{error}</div>;
  }

  return (
    <div>
      <h1>Yield Positions</h1>
      
      <div className="card">
        <h3 className="card-title">Active Yield Positions</h3>
        
        {yieldPositions.length === 0 ? (
          <p>No yield positions found</p>
        ) : (
          <table className="table">
            <thead>
              <tr>
                <th>Protocol</th>
                <th>Pool</th>
                <th>Blockchain</th>
                <th>Deposited Assets</th>
                <th>Total Value</th>
                <th>APY</th>
                <th>Daily Yield</th>
                <th>Entry Date</th>
              </tr>
            </thead>
            <tbody>
              {yieldPositions.map((position, index) => (
                <tr key={index}>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                      {position.protocol.logoUrl && (
                        <img 
                          src={position.protocol.logoUrl} 
                          alt={position.protocol.name} 
                          className="token-icon" 
                        />
                      )}
                      <span>{position.protocol.name}</span>
                    </div>
                  </td>
                  <td>{position.poolName}</td>
                  <td>{position.protocol.blockchain}</td>
                  <td>
                    {position.depositedTokens.map((token, i) => (
                      <div key={i}>
                        {token.balance} {token.token.symbol}
                      </div>
                    ))}
                  </td>
                  <td>{formatCurrency(position.totalValueUsd)}</td>
                  <td className="positive">{formatPercentage(position.apy)}</td>
                  <td>{formatCurrency(position.dailyYieldUsd)}</td>
                  <td>{formatDate(position.entryTime)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      
      <div className="card">
        <h3 className="card-title">Yield Summary</h3>
        
        <div className="dashboard" style={{ gridTemplateColumns: '1fr 1fr 1fr' }}>
          <div className="stat-card">
            <div className="stat-label">Total Value Locked</div>
            <div className="stat-value">
              {formatCurrency(
                yieldPositions.reduce((sum, pos) => sum + pos.totalValueUsd, 0)
              )}
            </div>
          </div>
          
          <div className="stat-card">
            <div className="stat-label">Daily Yield</div>
            <div className="stat-value">
              {formatCurrency(
                yieldPositions.reduce((sum, pos) => sum + pos.dailyYieldUsd, 0)
              )}
            </div>
          </div>
          
          <div className="stat-card">
            <div className="stat-label">Average APY</div>
            <div className="stat-value">
              {formatPercentage(
                yieldPositions.length > 0
                  ? yieldPositions.reduce((sum, pos) => sum + pos.apy, 0) / yieldPositions.length
                  : 0
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default YieldPositions; 