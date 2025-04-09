import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Chart as ChartJS, ArcElement, Tooltip, Legend, CategoryScale, LinearScale, PointElement, LineElement, Title } from 'chart.js';
import { Doughnut, Line } from 'react-chartjs-2';

// Register ChartJS components
ChartJS.register(ArcElement, Tooltip, Legend, CategoryScale, LinearScale, PointElement, LineElement, Title);

const Dashboard = ({ walletAddress }) => {
  const [portfolio, setPortfolio] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPortfolio = async () => {
      try {
        setLoading(true);
        const response = await axios.get(`/api/portfolio/${walletAddress}`);
        setPortfolio(response.data);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch portfolio:', err);
        setError('Failed to load portfolio data');
      } finally {
        setLoading(false);
      }
    };

    if (walletAddress) {
      fetchPortfolio();
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

  // Prepare data for token distribution chart
  const getTokenDistributionData = () => {
    const tokens = portfolio.tokenBalances.slice(0, 5); // Show top 5 tokens
    const otherTokens = portfolio.tokenBalances.slice(5);
    
    const otherValue = otherTokens.reduce((sum, token) => sum + token.balanceInUsd, 0);
    
    const data = {
      labels: [...tokens.map(t => t.token.symbol), 'Other'],
      datasets: [
        {
          data: [...tokens.map(t => t.balanceInUsd), otherValue],
          backgroundColor: [
            '#FF6384',
            '#36A2EB',
            '#FFCE56',
            '#4BC0C0',
            '#9966FF',
            '#C9CBCF'
          ],
          borderWidth: 1
        }
      ]
    };
    
    return data;
  };

  // Prepare data for yield positions chart
  const getYieldPositionsData = () => {
    const positions = portfolio.yieldPositions;
    
    const data = {
      labels: positions.map(p => p.protocol.name),
      datasets: [
        {
          data: positions.map(p => p.totalValueUsd),
          backgroundColor: [
            '#FF6384',
            '#36A2EB',
            '#FFCE56',
            '#4BC0C0',
            '#9966FF'
          ],
          borderWidth: 1
        }
      ]
    };
    
    return data;
  };

  // Prepare dummy data for portfolio history chart
  const getPortfolioHistoryData = () => {
    // This would normally come from the API with historical data
    const days = ['1 Week Ago', '6 Days Ago', '5 Days Ago', '4 Days Ago', '3 Days Ago', '2 Days Ago', 'Yesterday', 'Today'];
    
    // Create some simulated data based on current value
    const baseValue = portfolio.totalPortfolioValueUsd;
    const data = {
      labels: days,
      datasets: [
        {
          label: 'Portfolio Value (USD)',
          data: days.map((_, i) => 
            baseValue * (0.9 + (i * 0.02) + (Math.random() * 0.05))
          ),
          borderColor: '#2772db',
          backgroundColor: 'rgba(39, 114, 219, 0.1)',
          tension: 0.3,
          fill: true
        }
      ]
    };
    
    return data;
  };

  if (loading) {
    return <div className="loading">Loading portfolio data...</div>;
  }

  if (error) {
    return <div className="error">{error}</div>;
  }

  if (!portfolio) {
    return <div>No portfolio data available</div>;
  }

  return (
    <div>
      <h1>Portfolio Dashboard</h1>
      
      <div className="dashboard">
        <div className="stat-card">
          <div className="stat-label">Total Portfolio Value</div>
          <div className="stat-value">{formatCurrency(portfolio.totalPortfolioValueUsd)}</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Token Holdings</div>
          <div className="stat-value">{formatCurrency(portfolio.totalTokenValueUsd)}</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Yield Positions</div>
          <div className="stat-value">{formatCurrency(portfolio.totalYieldValueUsd)}</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Est. Daily Yield</div>
          <div className="stat-value">{formatCurrency(portfolio.estimatedDailyYieldUsd)}</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Est. Annual Yield</div>
          <div className="stat-value">{formatCurrency(portfolio.estimatedAnnualYieldUsd)}</div>
        </div>
        
        <div className="stat-card">
          <div className="stat-label">Average Portfolio APY</div>
          <div className="stat-value">{formatPercentage(portfolio.averagePortfolioApy)}</div>
        </div>
      </div>
      
      <div className="card portfolio-chart">
        <h3 className="card-title">Portfolio History</h3>
        <Line data={getPortfolioHistoryData()} />
      </div>
      
      <div className="charts-container" style={{ display: 'flex', gap: '1.5rem' }}>
        <div className="card" style={{ flex: 1 }}>
          <h3 className="card-title">Token Distribution</h3>
          {portfolio.tokenBalances.length > 0 ? (
            <Doughnut data={getTokenDistributionData()} />
          ) : (
            <p>No token balances found</p>
          )}
        </div>
        
        <div className="card" style={{ flex: 1 }}>
          <h3 className="card-title">Yield Positions</h3>
          {portfolio.yieldPositions.length > 0 ? (
            <Doughnut data={getYieldPositionsData()} />
          ) : (
            <p>No yield positions found</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 