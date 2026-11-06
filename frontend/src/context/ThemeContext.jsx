import React, { createContext, useContext, useState, useEffect } from 'react';

const ThemeContext = createContext(null);

export const ThemeProvider = ({ children }) => {
  const [themePreference, setThemePreference] = useState(() => {
    return localStorage.getItem('erp-theme-preference') || 'system';
  });

  const [activeTheme, setActiveTheme] = useState('light');

  useEffect(() => {
    const root = document.documentElement;

    const applyTheme = () => {
      let resolvedTheme = themePreference;
      if (themePreference === 'system') {
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        resolvedTheme = prefersDark ? 'dark' : 'light';
      }

      setActiveTheme(resolvedTheme);
      root.setAttribute('data-theme', resolvedTheme);
      if (resolvedTheme === 'dark') {
        root.classList.add('dark-theme');
      } else {
        root.classList.remove('dark-theme');
      }
    };

    applyTheme();

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const handleChange = () => {
      if (themePreference === 'system') {
        applyTheme();
      }
    };

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, [themePreference]);

  const setTheme = (mode) => {
    setThemePreference(mode);
    localStorage.setItem('erp-theme-preference', mode);
  };

  const toggleTheme = () => {
    const next = activeTheme === 'dark' ? 'light' : 'dark';
    setTheme(next);
  };

  return (
    <ThemeContext.Provider
      value={{
        themePreference,
        activeTheme,
        isDark: activeTheme === 'dark',
        setTheme,
        toggleTheme,
      }}
    >
      {children}
    </ThemeContext.Provider>
  );
};

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};
