import { useEffect } from 'react';

/**
 * Custom hook to set the page title in the format "Accounting | page_name"
 * @param pageName - The name of the current page
 */
export const usePageTitle = (pageName: string) => {
  useEffect(() => {
    const title = `Accounting | ${pageName}`;
    document.title = title;
    
    // Cleanup function to reset title when component unmounts
    return () => {
      document.title = 'Accounting';
    };
  }, [pageName]);
};

export default usePageTitle;
