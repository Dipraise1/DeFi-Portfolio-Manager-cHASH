import React from 'react';
import { Link } from 'react-router-dom';

const Header = ({ walletConnected, walletAddress, onDisconnect }) => {
  const shortenAddress = (address) => {
    return `${address.slice(0, 6)}...${address.slice(-4)}`;
  };

  return (
    <header className="header">
      <div className="logo">
        <i className="fas fa-chart-pie"></i>
        DeFi Portfolio Manager
      </div>
      
      {walletConnected && (
        <nav className="nav-links">
          <Link to="/">Dashboard</Link>
          <Link to="/tokens">Tokens</Link>
          <Link to="/yield">Yield</Link>
        </nav>
      )}
      
      {walletConnected && (
        <div className="wallet-info">
          <span className="wallet-address">
            {shortenAddress(walletAddress)}
          </span>
          <button className="disconnect-btn" onClick={onDisconnect}>
            Disconnect
          </button>
        </div>
      )}
    </header>
  );
};

export default Header; 