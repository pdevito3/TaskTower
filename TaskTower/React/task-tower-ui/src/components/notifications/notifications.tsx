import toast, { Toaster, ToastOptions } from "react-hot-toast";

export const Notification = () => {
  return (
    <Toaster
      position="top-right"
      gutter={6}
      containerClassName=""
      containerStyle={{}}
      toastOptions={{
        // Define default options
        className: "",
        duration: 5000,
        style: {
          background: "#363636",
          color: "#fff",
        },

        // Default options for specific types
        success: {
          duration: 2000,
          // className: "",
        },
      }}
    />
  );
};

Notification.success = (message: string, options?: ToastOptions) => {
  toast.success(message, options);
  // toast.custom(
  //   (t) => (
  //     // TODO framer motion
  //     <div className={`bg-white px-6 py-4 shadow-md rounded-full `}>
  //       Hello TailwindCSS! 👋
  //     </div>
  //   ),
  //   {
  //     duration: 1500,
  //   }
  // );
};
Notification.error = (message: string, options?: ToastOptions) => {
  toast.error(message, options);
};
